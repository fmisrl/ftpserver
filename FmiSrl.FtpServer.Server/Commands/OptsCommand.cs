using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class OptsCommand : IFtpCommand
{
    public string[] Verbs => ["OPTS"];

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
