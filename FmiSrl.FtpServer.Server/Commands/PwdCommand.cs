using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the PWD (Print Working Directory) command.
/// </summary>
public class PwdCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["PWD"];

    /// <inheritdoc/>
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
