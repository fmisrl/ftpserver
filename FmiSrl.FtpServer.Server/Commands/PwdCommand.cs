using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class PwdCommand : IFtpCommand
{
    public string[] Verbs => ["PWD"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (!context.Session.IsAuthenticated)
        {
            await context.Session.SendResponseAsync(530, "Not logged in.");
            return;
        }

        await context.Session.SendResponseAsync(257, $"\"{context.Session.CurrentDirectory}\" is current directory.");
    }
}
