﻿using ArchiveMaster.Configs;
using ArchiveMaster.Services;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArchiveMaster.Views
{
    /// <summary>
    /// UpdatePanel.xaml 的交互逻辑
    /// </summary>
    public partial class Step1Panel : OfflineSyncPanelBase
    {
        public Step1Panel(Step1ViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

    }
}