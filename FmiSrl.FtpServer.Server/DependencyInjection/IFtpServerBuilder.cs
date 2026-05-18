using FmiSrl.FtpServer.Server.Abstractions;
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

    /// <summary>
    /// Adds a middleware to the FTP command pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to add.</typeparam>
    /// <returns>The <see cref="IFtpServerBuilder"/> instance.</returns>
    IFtpServerBuilder AddMiddleware<TMiddleware>() where TMiddleware : class, IFtpCommandMiddleware;

    /// <summary>
    /// Adds an event handler to the FTP server.
    /// </summary>
    /// <typeparam name="TEventHandler">The type of the event handler to add.</typeparam>
    /// <returns>The <see cref="IFtpServerBuilder"/> instance.</returns>
    IFtpServerBuilder AddEventHandler<TEventHandler>() where TEventHandler : class, IFtpServerEventHandler;
}
