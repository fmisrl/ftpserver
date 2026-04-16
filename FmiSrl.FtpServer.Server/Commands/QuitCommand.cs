using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the QUIT command to terminate the FTP session.
/// </summary>
public class QuitCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["QUIT"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => false;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(221, "Goodbye.");
        // The actual connection closing should be handled by the session/server logic
    }
}
