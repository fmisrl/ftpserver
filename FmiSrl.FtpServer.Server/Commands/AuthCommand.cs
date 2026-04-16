using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the AUTH command.
/// </summary>
public class AuthCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["AUTH"];

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(502, "TLS/SSL not supported.");
    }
}
