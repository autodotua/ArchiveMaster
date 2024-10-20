﻿using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArchiveMaster
{
    public class FileBackupperModuleInitializer : IModuleInitializer
    {
        public string ModuleName => "文件备份服务";

        public int Order =>
#if DEBUG
            0;
#else
            5;
#endif

        public IList<ConfigInfo> Configs =>
        [
            new ConfigInfo(typeof(FileBackupperConfig)),
        ];

        public void RegisterServices(IServiceCollection services)
        {
            services.AddViewAndViewModel<BackupTasksPanel,BackupTasksViewModel>();
            services.AddViewAndViewModel<RestorePanel,RestoreViewModel>();

            services.AddHostedService<BackupBackgroundService>();
        }

        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(BackupTasksPanel), "任务列表", "备份任务的管理以及日志查看", baseUrl + "backup.svg"),
                new ToolPanelInfo(typeof(RestorePanel), "文件恢复", "恢复已备份的文件", baseUrl + "recovery.svg"),
            },
            GroupName = ModuleName
        };


        public void RegisterMessages(Visual visual)
        {
        }

        private readonly string baseUrl = "avares://ArchiveMaster.Module.FileBackupper/Assets/";
    }
}