using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the USER command for authentication initiation.
/// </summary>
public class UserCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["USER"];

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error in parameters or arguments.");
            return;
        }

        context.Session.Username = context.Arguments;
        await context.Session.SendResponseAsync(331, $"User {context.Arguments} OK. Password required.");
    }
}
