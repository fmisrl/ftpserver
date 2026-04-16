using System.Net;
using System.Net.Sockets;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the PASV (Passive Mode) command for data connections.
/// </summary>
public class PasvCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["PASV"];

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (!context.Session.IsAuthenticated)
        {
            await context.Session.SendResponseAsync(530, "Not logged in.");
            return;
        }

        await PrepareDataConnectionAsync(context);
    }

    private static async Task PrepareDataConnectionAsync(FtpCommandContext context)
    {
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

        await SendPassiveModeResponseAsync(context, dataConnection.Port);
    }

    private static async Task SendPassiveModeResponseAsync(FtpCommandContext context, int port)
    {
        string localIp = GetLocalIpAddress();
        
        var p1 = port / 256;
        var p2 = port % 256;

        await context.Session.SendResponseAsync(227, $"Entering Passive Mode ({localIp},{p1},{p2}).");
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            return TryDetermineLocalIp();
        }
        catch
        {
            return "127,0,0,1";
        }
    }

    private static string TryDetermineLocalIp()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        
        if (socket.LocalEndPoint is IPEndPoint endPoint)
        {
            return endPoint.Address.ToString().Replace('.', ',');
        }
        
        return "127,0,0,1";
    }
}
