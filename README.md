# FmiSrl.FtpServer

A lightweight, modern FTP server library for .NET.

## Features

- Fully asynchronous operation.
- Supports both Dependency Injection and manual instantiation.
- Extensible authentication and file system providers.
- Built-in support for common FTP commands (USER, PASS, LIST, RETR, STOR, etc.).
- PASV mode support.

## Installation

```bash
dotnet add package FmiSrl.FtpServer.Server
```

## Usage

### With Dependency Injection (Recommended)

To use the FTP server in a modern .NET application (e.g., ASP.NET Core, Worker Service), register it in your `Program.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using FmiSrl.FtpServer.Server;
using FmiSrl.FtpServer.Server.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add FTP Server with default configuration
builder.Services.AddFtpServer(options =>
{
    options.FtpPort = 21;
    options.ServerName = "My Custom FTP Server";
})
.UseFileSystemProvider(new PhysicalFileSystemProvider("./my_ftp_root"))
.UseAuthenticationProvider(new SimpleAuthenticationProvider("admin", "secure_password"));

var host = builder.Build();

// Get the server instance and start it
var ftpServer = host.Services.GetRequiredService<FtpServer>();
await ftpServer.StartAsync();

await host.RunAsync();
```

The server is registered as a **Singleton** by default.

### Without Dependency Injection

You can also use the library in simple console applications or legacy projects without a DI container:

```csharp
using FmiSrl.FtpServer.Server;
using FmiSrl.FtpServer.Server.Services;
using Microsoft.Extensions.Logging.Abstractions;

var options = new FtpServerConfigurationOptions
{
    FtpPort = 2121,
    ServerName = "Standalone FTP Server"
};

// Providers are strictly required
var ftpServer = new FtpServer(
    new PhysicalFileSystemProvider("./ftp_root"),
    new SimpleAuthenticationProvider("user", "pass"),
    options,
    NullLogger<FtpServer>.Instance
);

await ftpServer.StartAsync();
Console.WriteLine("Server started. Press any key to stop.");
Console.ReadKey();
await ftpServer.StopAsync();
```

## Configuration

### Logging

The library uses `Microsoft.Extensions.Logging`. When using DI, it will automatically use the configured logging providers (e.g., Serilog, Console, Debug).

When used manually, you can pass any `ILogger<FtpServer>` implementation to the constructor.

### Authentication Provider

Implement the `IAuthenticationProvider` interface to customize how users are authenticated:

```csharp
public interface IAuthenticationProvider
{
    Task<bool> AuthenticateAsync(string username, string password);
}
```

### File System Provider

Implement the `IFileSystemProvider` interface to provide custom storage (e.g., Azure Blob Storage, Database, S3):

```csharp
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
```
