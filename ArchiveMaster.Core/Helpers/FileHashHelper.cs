using System.Security.Cryptography;

namespace ArchiveMaster.Helpers;

public static class FileHashHelper
{
    public static async Task<string> CopyAndComputeSha1Async(string sourceFilePath, string destinationFilePath)
    {
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("源文件未找到", sourceFilePath);
        }

        // 确保目标目录存在
        string destinationDirectory = Path.GetDirectoryName(destinationFilePath);
        if (destinationDirectory != null && !Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        using var sha1 = SHA1.Create();
        await using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        await using var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
        
        byte[] buffer = new byte[4096];
        int bytesRead;

        while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await destinationStream.WriteAsync(buffer, 0, bytesRead);
            sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
        }

        sha1.TransformFinalBlock(buffer, 0, 0); // 完成哈希计算
        
        // 设置目标文件的最后写入时间与源文件相同
        File.SetLastWriteTime(destinationFilePath, File.GetLastWriteTime(sourceFilePath));

        return Convert.ToHexString(sha1.Hash);
    }

    public static async Task<string> ComputeSha1Async(string filePath)
    {
        using var sha1 = SHA1.Create();
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
            FileOptions.Asynchronous);
        byte[] hash = await sha1.ComputeHashAsync(stream);
        return Convert.ToHexString(hash);
    }
}