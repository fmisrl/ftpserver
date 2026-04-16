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
    public bool RequiresAuthentication => true;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {        await context.Session.SendResponseAsync(257, $"\"{context.Session.CurrentDirectory}\" is current directory.");
    }
}
