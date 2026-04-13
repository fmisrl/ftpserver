using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class NoopCommand : IFtpCommand
{
    public string[] Verbs => ["NOOP"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(200, "NOOP OK.");
    }
}
