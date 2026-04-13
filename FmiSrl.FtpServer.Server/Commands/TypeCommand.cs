using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class TypeCommand : IFtpCommand
{
    public string[] Verbs => ["TYPE"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(200, $"Type set to {context.Arguments}.");
    }
}
