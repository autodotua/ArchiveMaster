﻿using ArchiveMaster.ViewModels;
using Avalonia.Controls;
using FzLib;
using System.Collections.ObjectModel;

namespace ArchiveMaster.Views
{
    /// <summary>
    /// RebuildPanel.xaml 的交互逻辑
    /// </summary>
    public partial class Step3Panel : OfflineSyncPanelBase
    {

        public Step3Panel( )
        {
            ViewModel = new Step3ViewModel();
            DataContext = ViewModel;
            InitializeComponent();
        }

        public Step3ViewModel ViewModel { get; }
        
    }
}