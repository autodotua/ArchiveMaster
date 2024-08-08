﻿using PackingViewModel = ArchiveMaster.ViewModels.PackingViewModel;

namespace ArchiveMaster.Views
{
    public partial class PackingPanel : TwoStepPanelBase
    {
        public PackingPanel()
        {
            DataContext = new PackingViewModel();
            InitializeComponent();
        }
    }
}
