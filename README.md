# FmiSrl.FtpServer

![Build, Test and Publish](https://github.com/fmisrl/FmiSrl.FtpServer/actions/workflows/publish.yml/badge.svg)

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
using FmiSrl.FtpServer.Server.DependencyInjection;
using FmiSrl.FtpServer.Server.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure Options for Providers
builder.Services.Configure<PhysicalFileSystemProviderOptions>(opt =>
{
    opt.RootDirectory = "./my_ftp_root";
});

builder.Services.Configure<SimpleAuthenticationProviderOptions>(opt =>
{
    opt.Username = "admin";
    opt.Password = "secure_password";
});

// Add FTP Server and configure providers via generic DI
builder.Services.AddFtpServer(options =>
{
    options.FtpPort = 21;
    options.ServerName = "My Custom FTP Server";
})
.UseFileSystemProvider<PhysicalFileSystemProvider>()
.UseAuthenticationProvider<SimpleAuthenticationProvider>();

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
using Microsoft.Extensions.Options;

var serverOptions = Options.Create(new FtpServerConfigurationOptions
{
    FtpPort = 2121,
    ServerName = "Standalone FTP Server"
});

var fsOptions = Options.Create(new PhysicalFileSystemProviderOptions
{
    RootDirectory = "./ftp_root"
});

var authOptions = Options.Create(new SimpleAuthenticationProviderOptions
{
    Username = "user",
    Password = "password"
});

// Providers are strictly required
var ftpServer = new FtpServer(
    new PhysicalFileSystemProvider(fsOptions),
    new SimpleAuthenticationProvider(authOptions),
    serverOptions,
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

Implement the `IFileSystemProvider` interface to provide custom storage (e.g., Azure Blob Storage, Database, S3).
It provides an `FtpAuthenticationContext` which includes the currently authenticated username:

```csharp
using FmiSrl.FtpServer.Server.Abstractions;

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
```
