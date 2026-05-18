using FmiSrl.FtpServer.Server.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FmiSrl.FtpServer.Server.DependencyInjection;

internal sealed class FtpServerBuilder(IServiceCollection services) : IFtpServerBuilder
{
    public IServiceCollection Services { get; } = services;

    public IFtpServerBuilder AddMiddleware<TMiddleware>() where TMiddleware : class, IFtpCommandMiddleware
    {
        Services.AddTransient<IFtpCommandMiddleware, TMiddleware>();
        return this;
    }

    public IFtpServerBuilder AddEventHandler<TEventHandler>() where TEventHandler : class, IFtpServerEventHandler
    {
        Services.AddTransient<IFtpServerEventHandler, TEventHandler>();
        return this;
    }
}
