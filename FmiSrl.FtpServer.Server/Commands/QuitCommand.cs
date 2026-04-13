using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class QuitCommand : IFtpCommand
{
    public string[] Verbs => ["QUIT"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(221, "Goodbye.");
        // The actual connection closing should be handled by the session/server logic
    }
}
