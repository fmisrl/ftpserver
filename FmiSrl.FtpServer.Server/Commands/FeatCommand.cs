using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the FEAT command to list supported features.
/// </summary>
public class FeatCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["FEAT"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => false;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync("211-Extensions supported:\r\n PASV\r\n UTF8\r\n211 End\r\n");
    }
}
