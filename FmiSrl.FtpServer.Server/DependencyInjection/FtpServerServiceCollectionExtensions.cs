using FmiSrl.FtpServer.Server;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class FtpServerServiceCollectionExtensions
{
    /// <summary>
    /// Adds the FTP server and its dependencies to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure the FTP server options.</param>
    /// <returns>An <see cref="IFtpServerBuilder"/> to further configure the FTP server.</returns>
    public static IFtpServerBuilder AddFtpServer(this IServiceCollection services, Action<FtpServerConfigurationOptions>? configureOptions = null)
    {
        var options = new FtpServerConfigurationOptions();
        configureOptions?.Invoke(options);
        services.TryAddSingleton(options);

        services.TryAddSingleton<FtpServer>();

        // Default implementations
        services.TryAddSingleton<IFileSystemProvider>(sp => new PhysicalFileSystemProvider("./ftp_root"));
        services.TryAddSingleton<IAuthenticationProvider>(sp => new SimpleAuthenticationProvider("admin", "password"));

        return new FtpServerBuilder(services);
    }
}

public interface IFtpServerBuilder
{
    IServiceCollection Services { get; }
    IFtpServerBuilder UseFileSystemProvider<T>() where T : class, IFileSystemProvider;
    IFtpServerBuilder UseFileSystemProvider(IFileSystemProvider provider);
    IFtpServerBuilder UseAuthenticationProvider<T>() where T : class, IAuthenticationProvider;
    IFtpServerBuilder UseAuthenticationProvider(IAuthenticationProvider provider);
}

internal sealed class FtpServerBuilder : IFtpServerBuilder
{
    public IServiceCollection Services { get; }

    public FtpServerBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IFtpServerBuilder UseFileSystemProvider<T>() where T : class, IFileSystemProvider
    {
        Services.Replace(ServiceDescriptor.Singleton<IFileSystemProvider, T>());
        return this;
    }

    public IFtpServerBuilder UseFileSystemProvider(IFileSystemProvider provider)
    {
        Services.Replace(ServiceDescriptor.Singleton(provider));
        return this;
    }

    public IFtpServerBuilder UseAuthenticationProvider<T>() where T : class, IAuthenticationProvider
    {
        Services.Replace(ServiceDescriptor.Singleton<IAuthenticationProvider, T>());
        return this;
    }

    public IFtpServerBuilder UseAuthenticationProvider(IAuthenticationProvider provider)
    {
        Services.Replace(ServiceDescriptor.Singleton(provider));
        return this;
    }
}
