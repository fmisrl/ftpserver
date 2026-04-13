using System.Net;
using System.Net.Sockets;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;

namespace FmiSrl.FtpServer.Server.Commands;

public class PasvCommand : IFtpCommand
{
    public string[] Verbs => ["PASV"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (!context.Session.IsAuthenticated)
        {
            await context.Session.SendResponseAsync(530, "Not logged in.");
            return;
        }

        // Close existing data connection if any
        if (context.Session.DataConnection != null)
        {
            await context.Session.DataConnection.DisposeAsync();
        }

        var dataConnection = new MikuPassiveDataConnection(
            context.Configuration.ListeningIp,
            context.Configuration.PasvMinPort,
            context.Configuration.PasvMaxPort
        );
        context.Session.DataConnection = dataConnection;
        var port = dataConnection.Port;

        // Get local IP address to return to client
        string localIp = "127,0,0,1";
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            if (socket.LocalEndPoint is IPEndPoint endPoint)
            {
                localIp = endPoint.Address.ToString().Replace('.', ',');
            }
        }
        catch
        {
            // Fallback to 127,0,0,1 if we can't determine the local IP
        }
        
        var p1 = port / 256;
        var p2 = port % 256;

        await context.Session.SendResponseAsync(227, $"Entering Passive Mode ({localIp},{p1},{p2}).");
    }
}
