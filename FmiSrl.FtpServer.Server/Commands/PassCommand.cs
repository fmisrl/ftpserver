using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class PassCommand : IFtpCommand
{
    public string[] Verbs => ["PASS"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (context.Session.Username == null)
        {
            await context.Session.SendResponseAsync(503, "Bad sequence of commands.");
            return;
        }

        bool isAuthenticated = await context.Authenticator.AuthenticateAsync(context.Session.Username, context.Arguments);

        if (isAuthenticated)
        {
            context.Session.IsAuthenticated = true;
            await context.Session.SendResponseAsync(230, "User logged in, proceed.");
        }
        else
        {
            await context.Session.SendResponseAsync(530, "Not logged in.");
        }
    }
}
