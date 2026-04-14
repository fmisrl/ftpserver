namespace FmiSrl.FtpServer.Server.Abstractions;

public record FtpAuthenticationContext(string? Username);

public interface IFileSystemProvider
{
    Task<IEnumerable<FileSystemEntry>> GetEntriesAsync(FtpAuthenticationContext authContext, string path);
    Task<Stream> OpenReadAsync(FtpAuthenticationContext authContext, string path);
    Task<Stream> OpenWriteAsync(FtpAuthenticationContext authContext, string path);
    Task DeleteFileAsync(FtpAuthenticationContext authContext, string path);
    Task CreateDirectoryAsync(FtpAuthenticationContext authContext, string path);
    Task DeleteDirectoryAsync(FtpAuthenticationContext authContext, string path);
    Task<bool> FileExistsAsync(FtpAuthenticationContext authContext, string path);
    Task<bool> DirectoryExistsAsync(FtpAuthenticationContext authContext, string path);
    Task RenameAsync(FtpAuthenticationContext authContext, string oldPath, string newPath);
}

public record FileSystemEntry(string Name, long Size, DateTime LastModified, bool IsDirectory);
