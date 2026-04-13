using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class AuthCommand : IFtpCommand
{
    public string[] Verbs => ["AUTH"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(502, "TLS/SSL not supported.");
    }
}
