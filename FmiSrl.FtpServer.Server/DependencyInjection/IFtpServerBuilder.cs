using Microsoft.Extensions.DependencyInjection;

namespace FmiSrl.FtpServer.Server.DependencyInjection;

/// <summary>
/// A builder for configuring FTP server services.
/// </summary>
public interface IFtpServerBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> where the FTP server services are configured.
    /// </summary>
    /// <value>The <see cref="IServiceCollection"/> used by this builder.</value>
    IServiceCollection Services { get; }
}
