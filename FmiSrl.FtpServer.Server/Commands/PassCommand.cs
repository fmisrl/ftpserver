using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the PASS (Password) command for user authentication.
/// </summary>
public class PassCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["PASS"];

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (context.Session.Username == null)
        {
            await context.Session.SendResponseAsync(503, "Bad sequence of commands.");
            return;
        }

        var isAuthenticated = await context.Authenticator.AuthenticateAsync(context.Session.Username, context.Arguments);

        if (isAuthenticated)
        {
            context.Session.IsAuthenticated = true;
            await context.Session.SendResponseAsync(230, "User logged in, proceed.");
            return;
        }

        await context.Session.SendResponseAsync(530, "Not logged in.");
    }
}
