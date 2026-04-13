using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class FeatCommand : IFtpCommand
{
    public string[] Verbs => ["FEAT"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync("211-Extensions supported:\r\n PASV\r\n UTF8\r\n211 End\r\n");
    }
}
