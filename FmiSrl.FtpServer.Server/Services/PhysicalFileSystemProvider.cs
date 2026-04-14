using FmiSrl.FtpServer.Server.Abstractions;
using Microsoft.Extensions.Options;

namespace FmiSrl.FtpServer.Server.Services;

public class PhysicalFileSystemProvider(IOptions<PhysicalFileSystemProviderOptions> options) : IFileSystemProvider
{
    private readonly PhysicalFileSystemProviderOptions _options = options.Value;

    private string GetFullPath(FtpAuthenticationContext authContext, string path)
    {
        var username = authContext.Username ?? "anonymous";
        var userRootPath = Path.GetFullPath(Path.Combine(_options.RootDirectory, username));
        if (!Directory.Exists(userRootPath)) Directory.CreateDirectory(userRootPath);

        var normalizedPath = path.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        return Path.Combine(userRootPath, normalizedPath);
    }

    public Task<IEnumerable<FileSystemEntry>> GetEntriesAsync(FtpAuthenticationContext authContext, string path)
    {
        var fullPath = GetFullPath(authContext, path);
        var di = new DirectoryInfo(fullPath);
        if (!di.Exists) return Task.FromResult(Enumerable.Empty<FileSystemEntry>());

        var entries = di.GetFileSystemInfos()
            .Select(fsi => new FileSystemEntry(
            fsi.Name,
            fsi is FileInfo fi ? fi.Length : 0,
            fsi.LastWriteTime,
            fsi is DirectoryInfo
        ));

        return Task.FromResult(entries);
    }

    public Task<Stream> OpenReadAsync(FtpAuthenticationContext authContext, string path) =>
        Task.FromResult<Stream>(File.OpenRead(GetFullPath(authContext, path)));

    public Task<Stream> OpenWriteAsync(FtpAuthenticationContext authContext, string path)
    {
        var fullPath = GetFullPath(authContext, path);
        var dir = Path.GetDirectoryName(fullPath);
        if (dir != null && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return Task.FromResult<Stream>(File.OpenWrite(fullPath));
    }

    public Task DeleteFileAsync(FtpAuthenticationContext authContext, string path)
    {
        File.Delete(GetFullPath(authContext, path));
        return Task.CompletedTask;
    }

    public Task CreateDirectoryAsync(FtpAuthenticationContext authContext, string path)
    {
        Directory.CreateDirectory(GetFullPath(authContext, path));
        return Task.CompletedTask;
    }

    public Task DeleteDirectoryAsync(FtpAuthenticationContext authContext, string path)
    {
        Directory.Delete(GetFullPath(authContext, path), true);
        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(FtpAuthenticationContext authContext, string path) =>
        Task.FromResult(File.Exists(GetFullPath(authContext, path)));

    public Task<bool> DirectoryExistsAsync(FtpAuthenticationContext authContext, string path) =>
        Task.FromResult(Directory.Exists(GetFullPath(authContext, path)));

    public Task RenameAsync(FtpAuthenticationContext authContext, string oldPath, string newPath)
    {
        var oldFullPath = GetFullPath(authContext, oldPath);
        var newFullPath = GetFullPath(authContext, newPath);
        if (File.Exists(oldFullPath)) File.Move(oldFullPath, newFullPath);
        else Directory.Move(oldFullPath, newFullPath);
        return Task.CompletedTask;
    }
}
