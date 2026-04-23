using FmiSrl.FtpServer.Server;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FmiSrl.FtpServer.Tests.Unit.DependencyInjection;

public class DependencyInjectionTests
{
    [Fact]
    public void AddFtpServer_ShouldRegisterFtpServer_AndReturnBuilder()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddFtpServer();

        // Assert
        Assert.NotNull(builder);
        Assert.Same(services, builder.Services);
        
        var hasFtpServer = services.Any(d => d.ServiceType == typeof(FmiSrl.FtpServer.Server.FtpServer));
        Assert.True(hasFtpServer);
        
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<FtpServerConfigurationOptions>>();
        Assert.NotNull(options);
    }

    [Fact]
    public void AddFtpServer_WithConfigureOptions_ShouldRegisterOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFtpServer(options => { options.FtpPort = 2121; });
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<FtpServerConfigurationOptions>>().Value;

        // Assert
        Assert.Equal(2121, options.FtpPort);
    }

    [Fact]
    public void AddFtpServer_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddFtpServer());
    }

    private sealed class DummyFileSystemProvider : IFileSystemProvider
    {
        public Task<IEnumerable<FileSystemEntry>> GetEntriesAsync(FtpAuthenticationContext authContext, string path) => throw new NotImplementedException();
        public Task<Stream> OpenReadAsync(FtpAuthenticationContext authContext, string path) => throw new NotImplementedException();
        public Task<Stream> OpenWriteAsync(FtpAuthenticationContext authContext, string path) => throw new NotImplementedException();
        public Task DeleteFileAsync(FtpAuthenticationContext authContext, string path) => throw new NotImplementedException();
        public Task CreateDirectoryAsync(FtpAuthenticationContext authContext, string path) => throw new NotImplementedException();
        public Task DeleteDirectoryAsync(FtpAuthenticationContext authContext, string path) => throw new NotImplementedException();
        public Task<bool> FileExistsAsync(FtpAuthenticationContext authContext, string path) => throw new NotImplementedException();
        public Task<bool> DirectoryExistsAsync(FtpAuthenticationContext authContext, string path) => throw new NotImplementedException();
        public Task<FileSystemEntry?> GetEntryAsync(FtpAuthenticationContext authContext, string path) => throw new NotImplementedException();
        public Task RenameAsync(FtpAuthenticationContext authContext, string oldPath, string newPath) => throw new NotImplementedException();
    }

    private sealed class DummyAuthProvider : IAuthenticationProvider
    {
        public Task<bool> AuthenticateAsync(string username, string password) => throw new NotImplementedException();
    }

    [Fact]
    public void UseFileSystemProvider_Generic_ShouldReplaceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddFtpServer();

        // Act
        builder.UseFileSystemProvider<DummyFileSystemProvider>();

        // Assert
        var descriptor = services.Last(d => d.ServiceType == typeof(IFileSystemProvider));
        Assert.Equal(typeof(DummyFileSystemProvider), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void UseFileSystemProvider_Instance_ShouldReplaceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddFtpServer();
        var instance = new DummyFileSystemProvider();

        // Act
        builder.UseFileSystemProvider(instance);

        // Assert
        var descriptor = services.Last(d => d.ServiceType == typeof(IFileSystemProvider));
        Assert.Same(instance, descriptor.ImplementationInstance);
    }

    [Fact]
    public void UseAuthenticationProvider_Generic_ShouldReplaceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddFtpServer();

        // Act
        builder.UseAuthenticationProvider<DummyAuthProvider>();

        // Assert
        var descriptor = services.Last(d => d.ServiceType == typeof(IAuthenticationProvider));
        Assert.Equal(typeof(DummyAuthProvider), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void UseAuthenticationProvider_Instance_ShouldReplaceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddFtpServer();
        var instance = new DummyAuthProvider();

        // Act
        builder.UseAuthenticationProvider(instance);

        // Assert
        var descriptor = services.Last(d => d.ServiceType == typeof(IAuthenticationProvider));
        Assert.Same(instance, descriptor.ImplementationInstance);
    }
}
