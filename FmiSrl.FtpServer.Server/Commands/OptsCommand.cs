using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the OPTS (Options) command to configure connection parameters.
/// </summary>
public class OptsCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["OPTS"];

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (context.Arguments.Equals("UTF8 ON", StringComparison.OrdinalIgnoreCase))
        {
            await context.Session.SendResponseAsync(200, "UTF8 mode enabled.");
        }
        else
        {
            await context.Session.SendResponseAsync(501, "Option not recognized.");
        }
    }
}
