using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the NOOP (No Operation) command.
/// </summary>
public class NoopCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["NOOP"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => false;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(200, "NOOP OK.");
    }
}
