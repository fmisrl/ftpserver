using Microsoft.Extensions.DependencyInjection;

namespace FmiSrl.FtpServer.Server.DependencyInjection;

internal sealed class FtpServerBuilder(IServiceCollection services) : IFtpServerBuilder
{
    public IServiceCollection Services { get; } = services;
}
