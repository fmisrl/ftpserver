using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        var localIp = GetLocalIpAddress(context.Session.RemoteEndPoint);

        var p1 = port / 256;
        var p2 = port % 256;

        await context.Session.SendResponseAsync(227, $"Entering Passive Mode ({localIp},{p1},{p2}).");
    }

    private static string GetLocalIpAddress(EndPoint remoteEndPoint)
    {
        try
        {
            return TryDetermineLocalIp(remoteEndPoint);
        }
        catch
        {
            return "127,0,0,1";
        }
    }

    private static string TryDetermineLocalIp(EndPoint remoteEndPoint)
    {
        if (remoteEndPoint is not IPEndPoint ipEndPoint)
        {
            return "127,0,0,1";
        }

        var clientIp = ipEndPoint.Address;
        IPAddress? fallbackIp = null;

        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up ||
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            var ipProperties = networkInterface.GetIPProperties();
            var hasGateway = ipProperties.GatewayAddresses.Any();

            foreach (var ip in ipProperties.UnicastAddresses)
            {
                if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                {
                    continue;
                }

                // Check if client is in the same subnet
                if (!ip.IPv4Mask.Equals(IPAddress.Any))
                {
                    if (IsInSameSubnet(clientIp, ip.Address, ip.IPv4Mask))
                    {
                        return ip.Address.ToString().Replace('.', ',');
                    }
                }

                // Set fallback if this interface has a gateway
                if (hasGateway && fallbackIp == null)
                {
                    fallbackIp = ip.Address;
                }
            }
        }

        // If no direct subnet match, use the fallback (interface with a gateway)
        if (fallbackIp != null)
        {
            return fallbackIp.ToString().Replace('.', ',');
        }

        // If still no match, just try to get the first active IPv4
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                var ip = networkInterface.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;

                if (ip != null)
                {
                    return ip.ToString().Replace('.', ',');
                }
            }
        }

        return "127,0,0,1";
    }

    private static bool IsInSameSubnet(IPAddress address1, IPAddress address2, IPAddress subnetMask)
    {
        var ip1Bytes = address1.GetAddressBytes();
        var ip2Bytes = address2.GetAddressBytes();
        var maskBytes = subnetMask.GetAddressBytes();

        if (ip1Bytes.Length != ip2Bytes.Length || ip1Bytes.Length != maskBytes.Length)
        {
            return false;
        }

        for (var i = 0; i < ip1Bytes.Length; i++)
        {
            if ((ip1Bytes[i] & maskBytes[i]) != (ip2Bytes[i] & maskBytes[i]))
            {
                return false;
            }
        }

        return true;
    }
}
