using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using ArchiveMaster.ViewModels;
using ArchiveMaster.ViewModels.FileSystem;
using Microsoft.EntityFrameworkCore;

namespace ArchiveMaster.Utilities;

public class RestoreUtility(BackupTask task)
{
    public async Task<TreeDirInfo> GetSnapshotFileTreeAsync(int snapshotId, CancellationToken token = default)
    {
        await using var db = new DbService(task);
        TreeDirInfo tree = null;
        await Task.Run(() =>
        {
            var fileRecords = db.GetLatestFiles(snapshotId);

            tree = TreeDirInfo.CreateEmptyTree();

            foreach (var record in fileRecords.Select(p => new BackupFile(p)))
            {
                tree.AddFile(record);
            }
        }, token);
        return tree;
    }
}