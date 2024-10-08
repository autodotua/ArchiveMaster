﻿using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Messages;
using ArchiveMaster.Utilities;
using ArchiveMaster.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib;
using FzLib.Avalonia.Messages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels
{
    public abstract partial class
        OfflineSyncViewModelBase<TUtility, TConfig, TFile> : TwoStepViewModelBase<TUtility, TConfig>
        where TUtility : TwoStepUtilityBase<TConfig>
        where TConfig : ConfigBase
        where TFile : SimpleFileInfo
    {
        public OfflineSyncViewModelBase(TConfig config = null) : base(config)
        {
        }

        public OfflineSyncViewModelBase(bool enableInitialize, TConfig config = null) : base(config,
            enableInitialize)
        {
        }

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(Config))]
        private string configName;

        [ObservableProperty]
        private IList<string> configNames;

        public override void OnEnter()
        {
            string currentName = Services.Provider.GetRequiredService<OfflineSyncConfig>().CurrentConfigName;

            ConfigNames = Services.Provider.GetRequiredService<OfflineSyncConfig>().ConfigCollection.Keys.ToList();
            ConfigName = currentName;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AddedFileLength),
            nameof(AddedFileCount),
            nameof(ModifiedFileCount),
            nameof(ModifiedFileLength),
            nameof(DeletedFileCount),
            nameof(MovedFileCount),
            nameof(CheckedFileCount))]
        private ObservableCollection<TFile> files = new ObservableCollection<TFile>();

        public long AddedFileCount => Files?.Cast<SyncFileInfo>()
            .Where(p => p.UpdateType == FileUpdateType.Add && p.IsChecked)?.Count() ?? 0;

        public long AddedFileLength => Files?.Cast<SyncFileInfo>()
            .Where(p => p.UpdateType == FileUpdateType.Add && p.IsChecked)?.Sum(p => p.Length) ?? 0;

        public int CheckedFileCount => Files?.Where(p => p.IsChecked)?.Count() ?? 0;

        public int DeletedFileCount => Files?.Cast<SyncFileInfo>()
            .Where(p => p.UpdateType == FileUpdateType.Delete && p.IsChecked)?.Count() ?? 0;

        public char DirectorySeparatorChar => Path.DirectorySeparatorChar;

        public long ModifiedFileCount => Files?.Cast<SyncFileInfo>()
            .Where(p => p.UpdateType == FileUpdateType.Modify && p.IsChecked)?.Count() ?? 0;

        public long ModifiedFileLength => Files?.Cast<SyncFileInfo>()
            .Where(p => p.UpdateType == FileUpdateType.Modify && p.IsChecked)?.Sum(p => p.Length) ?? 0;

        public int MovedFileCount => Files?.Cast<SyncFileInfo>()
            .Where(p => p.UpdateType == FileUpdateType.Move && p.IsChecked)?.Count() ?? 0;

        [RelayCommand]
        private async Task AddConfigAsync()
        {
            if (await this.SendMessage(new InputDialogMessage()
                {
                    Type = InputDialogMessage.InputDialogType.Text,
                    Title = "新增配置",
                    DefaultValue = "新配置",
                    Validation = t =>
                    {
                        if (string.IsNullOrWhiteSpace(t))
                        {
                            throw new Exception("配置名为空");
                        }

                        if (ConfigNames.Contains(t))
                        {
                            throw new Exception("配置名已存在");
                        }
                    }
                }).Task is string result)
            {
                ConfigNames.Add(result);
                Services.Provider.GetRequiredService<OfflineSyncConfig>().ConfigCollection
                    .Add(result, new SingleConfig());
                ConfigName = result;
            }
        }

        private void AddFileCheckedNotify(SimpleFileInfo file)
        {
            file.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(SimpleFileInfo.IsChecked))
                {
                    return;
                }

                this.Notify(nameof(CheckedFileCount));
                if (s is not SyncFileInfo syncFile)
                {
                    return;
                }

                switch (syncFile.UpdateType)
                {
                    case FileUpdateType.Add:
                        this.Notify(nameof(AddedFileCount), nameof(AddedFileLength));
                        break;

                    case FileUpdateType.Modify:
                        this.Notify(nameof(ModifiedFileCount), nameof(ModifiedFileLength));
                        break;

                    case FileUpdateType.Delete:
                        this.Notify(nameof(DeletedFileCount));
                        break;

                    case FileUpdateType.Move:
                        this.Notify(nameof(MovedFileCount));
                        break;

                    case FileUpdateType.None:
                    default:
                        break;
                }
            };
        }

        partial void OnConfigNameChanged(string oldValue, string newValue)
        {
            if (Services.Provider.GetRequiredService<OfflineSyncConfig>().CurrentConfigName != newValue)
            {
                Services.Provider.GetRequiredService<OfflineSyncConfig>().CurrentConfigName = newValue;
            }

            ResetCommand.Execute(null);
        }

        partial void OnFilesChanged(ObservableCollection<TFile> value)
        {
            if (value == null)
            {
                return;
            }

            value.ForEach(p => AddFileCheckedNotify(p));
            value.CollectionChanged += (s, e) => throw new NotSupportedException("不允许对集合进行修改");
        }

        [RelayCommand]
        private async Task RemoveConfigAsync()
        {
            var name = ConfigName;
            var result = await this.SendMessage(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.YesNo,
                Title = "删除配置",
                Message = $"是否移除配置：{name}？"
            }).Task;
            if (result.Equals(true))
            {
                ConfigNames.Remove(name);
                Services.Provider.GetRequiredService<OfflineSyncConfig>().ConfigCollection.Remove(name);
                if (ConfigNames.Count == 0)
                {
                    ConfigNames.Add("默认");
                    ConfigName = "默认";
                }
                else
                {
                    ConfigName = ConfigNames[0];
                }
            }
        }

        [RelayCommand]
        private void SelectAll()
        {
            Files?.ForEach(p => p.IsChecked = true);
        }

        [RelayCommand]
        private void SelectNone()
        {
            Files?.ForEach(p => p.IsChecked = false);
        }

        protected override void OnReset()
        {
            Files = new ObservableCollection<TFile>();
        }
    }
}