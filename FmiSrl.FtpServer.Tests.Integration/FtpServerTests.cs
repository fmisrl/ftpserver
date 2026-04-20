using System.Net;
using FluentFTP;
using FmiSrl.FtpServer.Server;
using FmiSrl.FtpServer.Server.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FmiSrl.FtpServer.Tests.Integration;

public class FtpServerTests : IAsyncDisposable
{
    private static int _nextPort = 2121;
    private readonly FmiSrl.FtpServer.Server.FtpServer _server;
    private readonly string _rootPath;
    private readonly int _port;

    public FtpServerTests()
    {
        _port = Interlocked.Increment(ref _nextPort);
        _rootPath = Path.Combine(Path.GetTempPath(), "FtpServerTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootPath);

        var fsOptions = Options.Create(new PhysicalFileSystemProviderOptions
        {
            RootDirectory = _rootPath
        });
        var fileSystem = new PhysicalFileSystemProvider(fsOptions);

        var authOptions = Options.Create(new SimpleAuthenticationProviderOptions
        {
            Username = "test",
            Password = "password"
        });
        var authenticator = new SimpleAuthenticationProvider(authOptions);

        var serverOptions = Options.Create(new FtpServerConfigurationOptions
        {
            ListeningIp = "127.0.0.1",
            FtpPort = _port,
            PasvMinPort = 0,
            PasvMaxPort = 0,
            ServerName = "TestServer"
        });

        _server = new FmiSrl.FtpServer.Server.FtpServer(fileSystem, authenticator, serverOptions, NullLogger<FmiSrl.FtpServer.Server.FtpServer>.Instance);
    }

    public async ValueTask DisposeAsync()
    {
        await _server.StopAsync();
        if (Directory.Exists(_rootPath))
        {
            try
            {
                Directory.Delete(_rootPath, true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }

    [Fact]
    public async Task When_server_starts_should_be_able_to_login_and_list_files()
    {
        // Arrange
        await _server.StartAsync();
        
        // Create a test file for the user
        var userRoot = Path.Combine(_rootPath, "test");
        Directory.CreateDirectory(userRoot);
        File.WriteAllText(Path.Combine(userRoot, "test.txt"), "Hello World");

        using var client = new AsyncFtpClient("127.0.0.1", "test", "password", _port);
        
        // Act
        await client.Connect();
        var currentDir = await client.GetWorkingDirectory();
        var items = await client.GetListing();

        // Assert
        Assert.Equal("/", currentDir);
        Assert.Single(items);
        Assert.Equal("test.txt", items[0].Name);

        await client.Disconnect();
    }

    [Fact]
    public async Task When_invalid_credentials_should_fail_login()
    {
        // Arrange
        await _server.StartAsync();

        using var client = new AsyncFtpClient("127.0.0.1", "wrong", "password", _port);

        // Act & Assert
        await Assert.ThrowsAsync<FluentFTP.Exceptions.FtpAuthenticationException>(async () => await client.Connect());
    }

    [Fact]
    public async Task When_uploading_file_should_save_to_disk()
    {
        // Arrange
        await _server.StartAsync();
        using var client = new AsyncFtpClient("127.0.0.1", "test", "password", _port);
        await client.Connect();

        var testContent = "Test upload content";
        var contentBytes = System.Text.Encoding.UTF8.GetBytes(testContent);

        // Act
        var status = await client.UploadBytes(contentBytes, "/uploaded.txt");

        // Assert
        Assert.Equal(FtpStatus.Success, status);
        
        var filePath = Path.Combine(_rootPath, "test", "uploaded.txt");
        Assert.True(File.Exists(filePath));
        Assert.Equal(testContent, await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task When_downloading_file_should_retrieve_content()
    {
        // Arrange
        await _server.StartAsync();
        var userRoot = Path.Combine(_rootPath, "test");
        Directory.CreateDirectory(userRoot);
        var testContent = "Test download content";
        await File.WriteAllTextAsync(Path.Combine(userRoot, "download.txt"), testContent);

        using var client = new AsyncFtpClient("127.0.0.1", "test", "password", _port);
        await client.Connect();

        // Act
        var downloadedBytes = await client.DownloadBytes("/download.txt", CancellationToken.None);

        // Assert
        Assert.NotNull(downloadedBytes);
        Assert.Equal(testContent, System.Text.Encoding.UTF8.GetString(downloadedBytes));
    }

    [Fact]
    public async Task When_creating_and_removing_directory_should_update_disk()
    {
        // Arrange
        await _server.StartAsync();
        using var client = new AsyncFtpClient("127.0.0.1", "test", "password", _port);
        await client.Connect();
        var dirPath = "/newdir";

        // Act - Create
        await client.CreateDirectory(dirPath);

        // Assert - Create
        Assert.True(Directory.Exists(Path.Combine(_rootPath, "test", "newdir")));

        // Act - Remove
        await client.DeleteDirectory(dirPath);

        // Assert - Remove
        Assert.False(Directory.Exists(Path.Combine(_rootPath, "test", "newdir")));
    }

    [Fact]
    public async Task When_deleting_file_should_remove_from_disk()
    {
        // Arrange
        await _server.StartAsync();
        var userRoot = Path.Combine(_rootPath, "test");
        Directory.CreateDirectory(userRoot);
        var filePath = Path.Combine(userRoot, "todelete.txt");
        await File.WriteAllTextAsync(filePath, "delete me");

        using var client = new AsyncFtpClient("127.0.0.1", "test", "password", _port);
        await client.Connect();

        // Act
        await client.DeleteFile("/todelete.txt");

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task When_renaming_file_should_update_on_disk()
    {
        // Arrange
        await _server.StartAsync();
        var userRoot = Path.Combine(_rootPath, "test");
        Directory.CreateDirectory(userRoot);
        var filePath = Path.Combine(userRoot, "oldname.txt");
        await File.WriteAllTextAsync(filePath, "rename me");

        using var client = new AsyncFtpClient("127.0.0.1", "test", "password", _port);
        await client.Connect();

        // Act
        await client.Rename("/oldname.txt", "/newname.txt");

        // Assert
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(Path.Combine(userRoot, "newname.txt")));
    }
}
