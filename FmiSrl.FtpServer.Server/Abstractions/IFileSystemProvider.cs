namespace FmiSrl.FtpServer.Server.Abstractions;

public interface IFileSystemProvider
{
    Task<IEnumerable<FileSystemEntry>> GetEntriesAsync(string path);
    Task<Stream> OpenReadAsync(string path);
    Task<Stream> OpenWriteAsync(string path);
    Task DeleteFileAsync(string path);
    Task CreateDirectoryAsync(string path);
    Task DeleteDirectoryAsync(string path);
    Task<bool> FileExistsAsync(string path);
    Task<bool> DirectoryExistsAsync(string path);
    Task RenameAsync(string oldPath, string newPath);
}

public record FileSystemEntry(string Name, long Size, DateTime LastModified, bool IsDirectory);
