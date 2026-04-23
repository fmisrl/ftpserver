using System.Text;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Services;
using Microsoft.Extensions.Options;

namespace FmiSrl.FtpServer.Tests.Unit.Services;

public class PhysicalFileSystemProviderTests : IDisposable
{
    private readonly string _testRoot;
    private readonly PhysicalFileSystemProvider _provider;

    public PhysicalFileSystemProviderTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "FtpTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testRoot);

        var options = Options.Create(new PhysicalFileSystemProviderOptions
        {
            RootDirectory = _testRoot
        });

        _provider = new PhysicalFileSystemProvider(options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }
    }

    private static FtpAuthenticationContext CreateContext(string? username = "testuser") => new(username);

    [Fact]
    public async Task GetEntriesAsync_WhenDirectoryDoesNotExist_ShouldReturnEmpty()
    {
        var context = CreateContext();
        var entries = await _provider.GetEntriesAsync(context, "/nonexistent");
        
        Assert.Empty(entries);
    }

    [Fact]
    public async Task GetEntriesAsync_WhenDirectoryExists_ShouldReturnEntries()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(userRoot);
        File.WriteAllText(Path.Combine(userRoot, "file1.txt"), "hello");
        Directory.CreateDirectory(Path.Combine(userRoot, "dir1"));

        var entries = (await _provider.GetEntriesAsync(context, "/")).ToList();

        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.Name == "file1.txt" && !e.IsDirectory && e.Size == 5);
        Assert.Contains(entries, e => e.Name == "dir1" && e.IsDirectory);
    }

    [Fact]
    public async Task GetEntryAsync_WhenFileExists_ShouldReturnEntry()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(userRoot);
        File.WriteAllText(Path.Combine(userRoot, "file1.txt"), "hello");

        var entry = await _provider.GetEntryAsync(context, "/file1.txt");

        Assert.NotNull(entry);
        Assert.False(entry!.IsDirectory);
        Assert.Equal("file1.txt", entry.Name);
        Assert.Equal(5, entry.Size);
    }

    [Fact]
    public async Task GetEntryAsync_WhenDirectoryExists_ShouldReturnEntry()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(Path.Combine(userRoot, "dir1"));

        var entry = await _provider.GetEntryAsync(context, "/dir1");

        Assert.NotNull(entry);
        Assert.True(entry!.IsDirectory);
        Assert.Equal("dir1", entry.Name);
    }

    [Fact]
    public async Task GetEntryAsync_WhenNotExists_ShouldReturnNull()
    {
        var context = CreateContext();
        var entry = await _provider.GetEntryAsync(context, "/missing.txt");

        Assert.Null(entry);
    }

    [Fact]
    public async Task OpenReadAsync_WhenFileExists_ShouldReturnStream()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(userRoot);
        File.WriteAllText(Path.Combine(userRoot, "file1.txt"), "hello");

        await using var stream = await _provider.OpenReadAsync(context, "/file1.txt");
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        Assert.Equal("hello", content);
    }

    [Fact]
    public async Task OpenWriteAsync_ShouldCreateFileAndReturnStream()
    {
        var context = CreateContext();
        
        await using (var stream = await _provider.OpenWriteAsync(context, "/newdir/file2.txt"))
        {
            var bytes = Encoding.UTF8.GetBytes("world");
            await stream.WriteAsync(bytes);
        }

        var userRoot = Path.Combine(_testRoot, "testuser");
        var filePath = Path.Combine(userRoot, "newdir", "file2.txt");
        Assert.True(File.Exists(filePath));
        Assert.Equal("world", File.ReadAllText(filePath));
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileExists_ShouldDeleteFile()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(userRoot);
        var filePath = Path.Combine(userRoot, "file1.txt");
        File.WriteAllText(filePath, "hello");

        await _provider.DeleteFileAsync(context, "/file1.txt");

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task CreateDirectoryAsync_ShouldCreateDirectory()
    {
        var context = CreateContext();
        
        await _provider.CreateDirectoryAsync(context, "/newdir");

        var userRoot = Path.Combine(_testRoot, "testuser");
        Assert.True(Directory.Exists(Path.Combine(userRoot, "newdir")));
    }

    [Fact]
    public async Task DeleteDirectoryAsync_ShouldDeleteDirectory()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        var dirPath = Path.Combine(userRoot, "dir1");
        Directory.CreateDirectory(dirPath);

        await _provider.DeleteDirectoryAsync(context, "/dir1");

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public async Task FileExistsAsync_WhenFileExists_ShouldReturnTrue()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(userRoot);
        File.WriteAllText(Path.Combine(userRoot, "file1.txt"), "hello");

        var exists = await _provider.FileExistsAsync(context, "/file1.txt");
        Assert.True(exists);
    }

    [Fact]
    public async Task FileExistsAsync_WhenDirectoryExists_ShouldReturnFalse()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(Path.Combine(userRoot, "dir1"));

        var exists = await _provider.FileExistsAsync(context, "/dir1");
        Assert.False(exists);
    }

    [Fact]
    public async Task DirectoryExistsAsync_WhenDirectoryExists_ShouldReturnTrue()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(Path.Combine(userRoot, "dir1"));

        var exists = await _provider.DirectoryExistsAsync(context, "/dir1");
        Assert.True(exists);
    }

    [Fact]
    public async Task DirectoryExistsAsync_WhenFileExists_ShouldReturnFalse()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(userRoot);
        File.WriteAllText(Path.Combine(userRoot, "file1.txt"), "hello");

        var exists = await _provider.DirectoryExistsAsync(context, "/file1.txt");
        Assert.False(exists);
    }

    [Fact]
    public async Task RenameAsync_WhenFile_ShouldRenameFile()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(userRoot);
        File.WriteAllText(Path.Combine(userRoot, "file1.txt"), "hello");

        await _provider.RenameAsync(context, "/file1.txt", "/file2.txt");

        Assert.False(File.Exists(Path.Combine(userRoot, "file1.txt")));
        Assert.True(File.Exists(Path.Combine(userRoot, "file2.txt")));
    }

    [Fact]
    public async Task RenameAsync_WhenDirectory_ShouldRenameDirectory()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(Path.Combine(userRoot, "dir1"));

        await _provider.RenameAsync(context, "/dir1", "/dir2");

        Assert.False(Directory.Exists(Path.Combine(userRoot, "dir1")));
        Assert.True(Directory.Exists(Path.Combine(userRoot, "dir2")));
    }

    [Fact]
    public async Task GetFileSizeAsync_WhenFileExists_ShouldReturnSize()
    {
        var context = CreateContext();
        var userRoot = Path.Combine(_testRoot, "testuser");
        Directory.CreateDirectory(userRoot);
        File.WriteAllText(Path.Combine(userRoot, "file1.txt"), "hello");

        var size = await _provider.GetFileSizeAsync(context, "/file1.txt");

        Assert.Equal(5, size);
    }

    [Fact]
    public async Task GetFileSizeAsync_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
    {
        var context = CreateContext();

        await Assert.ThrowsAsync<FileNotFoundException>(() => _provider.GetFileSizeAsync(context, "/missing.txt"));
    }

    [Fact]
    public async Task AnonymousUser_ShouldUseAnonymousDirectory()
    {
        var context = CreateContext(null);
        await _provider.CreateDirectoryAsync(context, "/anon_dir");

        Assert.True(Directory.Exists(Path.Combine(_testRoot, "anonymous", "anon_dir")));
    }

    [Fact]
    public async Task PathTraversal_ShouldThrowUnauthorizedAccessException()
    {
        var context = CreateContext();
        
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _provider.GetEntriesAsync(context, "/../../windows/system32"));
    }
}