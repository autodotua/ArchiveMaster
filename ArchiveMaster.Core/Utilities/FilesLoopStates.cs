namespace ArchiveMaster.Utilities;

public class FilesLoopStates(FilesLoopOptions options)
{
    public FilesLoopOptions Options { get; } = options;
    private long totalLength = 0;
    private int fileCount = 0;
    private int fileIndex = 0;
    private long fileLength = 0;

    internal bool NeedBroken { get; private set; }

    public int FileCount
    {
        get => CanAccessFileCount ? fileCount : throw new ArgumentException("未初始化文件总数，不可调用TotalLength");
        internal set
        {
            fileCount = value;
            CanAccessFileCount = true;
        }
    }

    public int FileIndex => fileIndex;

    public long TotalLength
    {
        get => CanAccessTotalLength ? totalLength : throw new ArgumentException("未初始化总大小，不可调用TotalLength");
        internal set
        {
            totalLength = value;
            CanAccessTotalLength = true;
        }
    }

    internal bool CanAccessTotalLength { get; set; }

    internal bool CanAccessFileCount { get; set; }

    public long AccumulatedLength => fileLength;

    public static string ProgressMessageFormat { get; set; } = "（{0}/{1}）";

    public static string ProgressMessageIndexOnlyFormat { get; set; } = "（{0}个）";

    public void IncreaseFileIndex()
    {
        if (options.Threads != 1)
        {
            Interlocked.Increment(ref fileIndex);
        }
        else
        {
            fileIndex++;
        }
    }

    public void IncreaseFileLength(long increment)
    {
        if (options.Threads != 1)
        {
            Interlocked.Add(ref fileLength, increment);
        }
        else
        {
            fileLength += increment;
        }
    }

    public string GetFileNumberMessage()
    {
        int fileIndex = FileIndex + 1;
        if (CanAccessFileCount)
        {
            return string.Format(ProgressMessageFormat, fileIndex, FileCount);
        }

        return string.Format(ProgressMessageIndexOnlyFormat, fileIndex);
    }

    public void Break()
    {
        NeedBroken = true;
    }
}