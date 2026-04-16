using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the TYPE command to set the transfer mode (e.g., ASCII, Binary).
/// </summary>
public class TypeCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["TYPE"];

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(200, $"Type set to {context.Arguments}.");
    }
}
