using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the SYST (System) command to query the server's operating system type.
/// </summary>
public class SystCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["SYST"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => false;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(215, "UNIX Type: L8");
    }
}
