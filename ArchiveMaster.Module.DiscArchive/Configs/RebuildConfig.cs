using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class RebuildConfig : ConfigBase
{
    [ObservableProperty]
    private string discDirs;

    [ObservableProperty]
    private string targetDir;

    [ObservableProperty]
    private bool skipIfExisted = true;

    [ObservableProperty]
    private int maxTimeToleranceSecond = 2;

    [ObservableProperty]
    private bool checkOnly;


    public override void Check()
    {
        CheckEmpty(DiscDirs, "光盘目录");
        CheckEmpty(TargetDir, "目标目录");
    }
}