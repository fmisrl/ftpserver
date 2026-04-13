using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Services;

public class PhysicalFileSystemProvider : IFileSystemProvider
{
    private readonly string _rootPath;

    public PhysicalFileSystemProvider(string rootPath)
    {
        _rootPath = Path.GetFullPath(rootPath);
    }

    private string GetFullPath(string path)
    {
        var normalizedPath = path.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        return Path.Combine(_rootPath, normalizedPath);
    }

    public Task<IEnumerable<FileSystemEntry>> GetEntriesAsync(string path)
    {
        var fullPath = GetFullPath(path);
        var di = new DirectoryInfo(fullPath);
        var entries = di.GetFileSystemInfos().Select(fsi => new FileSystemEntry(
            fsi.Name,
            fsi is FileInfo fi ? fi.Length : 0,
            fsi.LastWriteTime,
            fsi is DirectoryInfo
        ));
        return Task.FromResult(entries);
    }

    public Task<Stream> OpenReadAsync(string path) => Task.FromResult<Stream>(File.OpenRead(GetFullPath(path)));

    public Task<Stream> OpenWriteAsync(string path) => Task.FromResult<Stream>(File.OpenWrite(GetFullPath(path)));

    public Task DeleteFileAsync(string path)
    {
        File.Delete(GetFullPath(path));
        return Task.CompletedTask;
    }

    public Task CreateDirectoryAsync(string path)
    {
        Directory.CreateDirectory(GetFullPath(path));
        return Task.CompletedTask;
    }

    public Task DeleteDirectoryAsync(string path)
    {
        Directory.Delete(GetFullPath(path), true);
        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string path) => Task.FromResult(File.Exists(GetFullPath(path)));

    public Task<bool> DirectoryExistsAsync(string path) => Task.FromResult(Directory.Exists(GetFullPath(path)));

    public Task RenameAsync(string oldPath, string newPath)
    {
        var oldFullPath = GetFullPath(oldPath);
        var newFullPath = GetFullPath(newPath);
        if (File.Exists(oldFullPath)) File.Move(oldFullPath, newFullPath);
        else Directory.Move(oldFullPath, newFullPath);
        return Task.CompletedTask;
    }
}
