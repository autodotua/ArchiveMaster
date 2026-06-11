using FzLib.Avalonia.Dialogs;
using FzLib.IO;

namespace ArchiveMaster.Views;

public class FileFilterDialog : DialogHost
{
    public FileFilterDialog(FileFilterRule filterRule)
    {
        Title = "筛选规则";
        PrimaryButtonContent = "确定";
        Content = new FileFilterPanel() { Filter = filterRule };
    }

    protected override void OnPrimaryButtonClick()
    {
        base.OnPrimaryButtonClick();
        Close();
    }
}