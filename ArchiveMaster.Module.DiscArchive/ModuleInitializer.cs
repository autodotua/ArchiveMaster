﻿using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster
{
    public class DiscArchiveModuleInitializer : IModuleInitializer
    {
        public string ModuleName => "光盘归档工具";
        
        public int Order => 4;

        public IList<ConfigInfo> Configs =>
        [
            new ConfigInfo(typeof(PackingConfig)),
            new ConfigInfo(typeof(RebuildConfig)),
        ];
        
        public void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<PackingViewModel>();
            services.AddTransient<RebuildViewModel>();

            services.AddTransient<PackingPanel>();
            services.AddTransient<RebuildPanel>();

            services.AddTransient<PackingService>();
            services.AddTransient<RebuildService>();
        }
        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(PackingPanel), "打包到光盘", "将文件按照修改时间顺序，根据光盘最大容量制作成若干文件包", baseUrl + "disc.svg"),
                new ToolPanelInfo(typeof(RebuildPanel), "从光盘重建", "从备份的光盘冲提取文件并恢复为原始目录结构", baseUrl + "rebuild.svg"),
                //
            },
            GroupName = ModuleName
        };


        public void RegisterMessages(Visual visual)
        {
        }

        private readonly string baseUrl = "avares://ArchiveMaster.Module.DiscArchive/Assets/";
    }
}