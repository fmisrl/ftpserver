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
