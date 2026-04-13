using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class SystCommand : IFtpCommand
{
    public string[] Verbs => ["SYST"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(215, "UNIX Type: L8");
    }
}
