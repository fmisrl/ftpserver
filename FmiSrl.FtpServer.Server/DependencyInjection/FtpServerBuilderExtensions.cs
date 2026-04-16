using FmiSrl.FtpServer.Server.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FmiSrl.FtpServer.Server.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IFtpServerBuilder"/>.
/// </summary>
public static class FtpServerBuilderExtensions
{
    /// <summary>
    /// Configures the FTP server to use the specified file system provider.
    /// </summary>
    /// <typeparam name="T">The type of the file system provider.</typeparam>
    /// <param name="builder">The <see cref="IFtpServerBuilder"/> to configure.</param>
    /// <returns>The <see cref="IFtpServerBuilder"/> instance.</returns>
    public static IFtpServerBuilder UseFileSystemProvider<T>(this IFtpServerBuilder builder)
        where T : class, IFileSystemProvider
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IFileSystemProvider, T>());
        return builder;
    }

    /// <summary>
    /// Configures the FTP server to use the specified file system provider instance.
    /// </summary>
    /// <param name="builder">The <see cref="IFtpServerBuilder"/> to configure.</param>
    /// <param name="provider">The <see cref="IFileSystemProvider"/> instance to use.</param>
    /// <returns>The <see cref="IFtpServerBuilder"/> instance.</returns>
    public static IFtpServerBuilder UseFileSystemProvider(this IFtpServerBuilder builder, IFileSystemProvider provider)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton(provider));
        return builder;
    }

    /// <summary>
    /// Configures the FTP server to use the specified authentication provider.
    /// </summary>
    /// <typeparam name="T">The type of the authentication provider.</typeparam>
    /// <param name="builder">The <see cref="IFtpServerBuilder"/> to configure.</param>
    /// <returns>The <see cref="IFtpServerBuilder"/> instance.</returns>
    public static IFtpServerBuilder UseAuthenticationProvider<T>(this IFtpServerBuilder builder)
        where T : class, IAuthenticationProvider
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationProvider, T>());
        return builder;
    }

    /// <summary>
    /// Configures the FTP server to use the specified authentication provider instance.
    /// </summary>
    /// <param name="builder">The <see cref="IFtpServerBuilder"/> to configure.</param>
    /// <param name="provider">The <see cref="IAuthenticationProvider"/> instance to use.</param>
    /// <returns>The <see cref="IFtpServerBuilder"/> instance.</returns>
    public static IFtpServerBuilder UseAuthenticationProvider(
        this IFtpServerBuilder builder,
        IAuthenticationProvider provider
    )
    {
        builder.Services.Replace(ServiceDescriptor.Singleton(provider));
        return builder;
    }
}
