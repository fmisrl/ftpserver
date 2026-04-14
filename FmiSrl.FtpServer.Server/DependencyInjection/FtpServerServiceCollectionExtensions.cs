using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FmiSrl.FtpServer.Server.DependencyInjection;

/// <summary>
/// Extension methods for setting up FTP server services in an <see cref="IServiceCollection" />.
/// </summary>
public static class FtpServerServiceCollectionExtensions
{
    /// <summary>
    /// Adds the FTP server services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configureOptions">An <see cref="Action{FtpServerConfigurationOptions}"/> to configure the provided <see cref="FtpServerConfigurationOptions"/>.</param>
    /// <returns>An <see cref="IFtpServerBuilder"/> that can be used to further configure the FTP server.</returns>
    public static IFtpServerBuilder AddFtpServer(
        this IServiceCollection services,
        Action<FtpServerConfigurationOptions>? configureOptions = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<FtpServerConfigurationOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.TryAddSingleton<FtpServer>();

        return new FtpServerBuilder(services);
    }
}

/// <summary>
/// A builder for configuring FTP server services.
/// </summary>
public interface IFtpServerBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> where the FTP server services are configured.
    /// </summary>
    IServiceCollection Services { get; }
}

internal sealed class FtpServerBuilder(IServiceCollection services) : IFtpServerBuilder
{
    public IServiceCollection Services { get; } = services;
}

/// <summary>
/// Extension methods for <see cref="IFtpServerBuilder"/>.
/// </summary>
public static class FtpServerBuilderExtensions
{
    /// <summary>
    /// Configures the FTP server to use the specified file system provider.
    /// </summary>
    public static IFtpServerBuilder UseFileSystemProvider<T>(this IFtpServerBuilder builder)
        where T : class, IFileSystemProvider
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IFileSystemProvider, T>());
        return builder;
    }

    /// <summary>
    /// Configures the FTP server to use the specified file system provider instance.
    /// </summary>
    public static IFtpServerBuilder UseFileSystemProvider(this IFtpServerBuilder builder, IFileSystemProvider provider)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton(provider));
        return builder;
    }

    /// <summary>
    /// Configures the FTP server to use the specified authentication provider.
    /// </summary>
    public static IFtpServerBuilder UseAuthenticationProvider<T>(this IFtpServerBuilder builder)
        where T : class, IAuthenticationProvider
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationProvider, T>());
        return builder;
    }

    /// <summary>
    /// Configures the FTP server to use the specified authentication provider instance.
    /// </summary>
    public static IFtpServerBuilder UseAuthenticationProvider(
        this IFtpServerBuilder builder,
        IAuthenticationProvider provider
    )
    {
        builder.Services.Replace(ServiceDescriptor.Singleton(provider));
        return builder;
    }
}
