﻿using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ArchiveMaster.Basic;
using ArchiveMaster.Converters;
using ArchiveMaster.Models;
using ArchiveMaster.ViewModels.FileSystem;
using ArchiveMaster.Views;
using Avalonia.Platform.Storage;
using FzLib.Avalonia.Messages;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.ViewModels
{
    public partial class BackupManageCenterViewModel : ViewModelBase
    {
        private AppConfig appConfig;

        [ObservableProperty]
        private bool canSelectTasks = true;

        [ObservableProperty]
        private ObservableCollection<BackupLogEntity> logs;

        [ObservableProperty]
        private SimpleFileInfo selectedFile;

        [ObservableProperty]
        private BackupSnapshotWithFileCount selectedSnapshot;

        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty]
        private BackupTask selectedTask;
        [ObservableProperty]
        private ObservableCollection<BackupSnapshotWithFileCount> snapshots;

        [ObservableProperty]
        private ObservableCollection<BackupTask> tasks;

        [ObservableProperty]
        private BulkObservableCollection<SimpleFileInfo> treeFiles;

        public BackupManageCenterViewModel(FileBackupperConfig config, AppConfig appConfig)
        {
            Config = config;
            this.appConfig = appConfig;
        }

        public FileBackupperConfig Config { get; }

        public override async void OnEnter()
        {
            Tasks = new ObservableCollection<BackupTask>(Config.Tasks);
            await Tasks.UpdateStatusAsync();
        }

        [RelayCommand]
        private async Task JumpToLogsBySnapshotAsync(BackupSnapshotWithFileCount snapshot)
        {
            SelectedTabIndex = 2;
            await using var db = new DbService(SelectedTask);
            Logs = new ObservableCollection<BackupLogEntity>(await db.GetLogsAsync(snapshot.Snapshot.Id));
        }

        [RelayCommand]
        private async Task JumpToRestoreBySnapshotAsync(BackupSnapshotWithFileCount snapshot)
        {
            SelectedTabIndex = 1;
            var utility = new RestoreUtility(SelectedTask);
            var tree = await utility.GetSnapshotFileTreeAsync(snapshot.Snapshot.Id);
            tree.Reorder();
            tree.Name = $"快照{snapshot.Snapshot.BeginTime}";

            TreeFiles = new BulkObservableCollection<SimpleFileInfo>();
            TreeFiles.Add(tree);
        }

        async partial void OnSelectedTaskChanged(BackupTask value)
        {
            Snapshots = null;
            Logs = null;
            TreeFiles = null;

            if (value != null)
            {
                try
                {
                    CanSelectTasks = false;
                    DbService db = new DbService(value);
                    Snapshots = new ObservableCollection<BackupSnapshotWithFileCount>(
                        await db.GetSnapshotsWithFilesAsync());
                }
                catch (Exception ex)
                {
                    await this.ShowErrorAsync("加载快照失败", ex);
                }
                finally
                {
                    CanSelectTasks = true;
                }
            }
        }
        [RelayCommand]
        private async Task SaveAsAsync()
        {
            switch (SelectedFile)
            {
                case BackupFile file:
                    await SaveFile(file);
                    break;
                case TreeDirInfo dir:
                    await SaveFolder(dir);
                    break;
            }
        }

        private async Task SaveFile(BackupFile file)
        {
            if (file.RecordEntity.PhysicalFile == null)
            {
                await this.ShowErrorAsync("备份文件不存在", "该文件不存在实际备份文件，可能是由虚拟快照生成");
                return;
            }

            var extension = Path.GetExtension(file.Name).TrimStart('.');
            var saveFile = await this.SendMessage(new GetStorageProviderMessage()).StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions()
                {
                    DefaultExtension = extension,
                    SuggestedFileName = file.Name,
                    FileTypeChoices =
                    [
                        new FilePickerFileType($"{extension}文件")
                            { Patterns = [$"*.{(extension.Length == 0 ? "*" : extension)}"] }
                    ]
                });
            var path = saveFile?.TryGetLocalPath();
            if (path != null)
            {
                var dialog = new FileProgressDialog();
                this.SendMessage(new DialogHostMessage(dialog));
                string backupFile = Path.Combine(SelectedTask.BackupDir, file.RecordEntity.PhysicalFile.FileName);
                if (!File.Exists(backupFile))
                {
                    await this.ShowErrorAsync("备份文件不存在", "该文件不存在实际备份文件，可能是文件丢失");
                    return;
                }

                await dialog.CopyFileAsync(backupFile, path, file.Time);
            }
        }

        private async Task SaveFolder(TreeDirInfo dir)
        {
            var folders = await this.SendMessage(new GetStorageProviderMessage()).StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions());
            if (folders is { Count: 1 })
            {
                var rootDir = folders[0].TryGetLocalPath();
                var dialog = new FileProgressDialog();
                this.SendMessage(new DialogHostMessage(dialog));
                var files = dir.Flatten();
                List<string> sourcePaths = new List<string>();
                List<string> destinationPaths = new List<string>();
                List<DateTime> times = new List<DateTime>();
                foreach (var file in files.Cast<BackupFile>())
                {
                    string backupFile = Path.Combine(SelectedTask.BackupDir, file.RecordEntity.PhysicalFile.FileName);
                    string fileRelativePath = dir.RelativePath == null
                        ? file.RelativePath
                        : Path.GetRelativePath(dir.RelativePath, file.RelativePath);
                    string destinationPath = Path.Combine(rootDir, fileRelativePath);
                    sourcePaths.Add(backupFile);
                    destinationPaths.Add(destinationPath);
                    times.Add(file.Time);

                    if (!File.Exists(backupFile))
                    {
                        await this.ShowErrorAsync("备份文件不存在", "该文件不存在实际备份文件，可能是文件丢失");
                        return;
                    }
                }

                await dialog.CopyFilesAsync(sourcePaths, destinationPaths, times);
            }
        }

        [RelayCommand]
        private Task ShowDetailAsync(BackupLogEntity log)
        {
            return this.SendMessage(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Ok,
                Message = log.Message,
                Title = LogLevelConverter.GetDescription(log.Type),
                Detail = log.Detail
            }).Task;
        }

        [RelayCommand]
        private async Task TestFullBackupAsync()
        {
            BackupUtility utility = new BackupUtility(SelectedTask);
            await utility.FullBackupAsync(false);
        }

        [RelayCommand]
        private async Task TestIncrementalBackupAsync()
        {
            BackupUtility utility = new BackupUtility(SelectedTask);
            await utility.IncrementalBackupAsync();
        }
    }
}