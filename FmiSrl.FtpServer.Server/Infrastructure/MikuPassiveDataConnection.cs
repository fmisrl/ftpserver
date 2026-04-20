using System.Net;
using System.Net.Sockets;
using Miku.Core;
using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Infrastructure;

/// <summary>
/// Provides an implementation of <see cref="IFtpDataConnection"/> that uses a passive connection.
/// </summary>
public class MikuPassiveDataConnection : IFtpDataConnection
{
    private NetServer? _dataServer;
    private MikuClientStream? _stream;
    private readonly TaskCompletionSource<Stream> _streamTcs = new();

    /// <summary>
    /// Gets the port used for the passive data connection.
    /// </summary>
    /// <value>The port number as an <see cref="int"/>.</value>
    public int Port { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MikuPassiveDataConnection"/> class.
    /// </summary>
    /// <param name="ip">The IP address to bind the data server to.</param>
    /// <param name="minPort">The minimum port number in the range of allowed ports.</param>
    /// <param name="maxPort">The maximum port number in the range of allowed ports.</param>
    /// <param name="authorizedClientIp">The IP address authorized to connect to this data port.</param>
    public MikuPassiveDataConnection(
        string ip,
        int minPort,
        int maxPort,
        string authorizedClientIp
    )
    {
        // Try to find a free port in the specified range
        var bound = false;

        for (var portToTry = minPort; portToTry <= maxPort; portToTry++)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Any, portToTry);
                listener.Start();
                Port = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
                bound = true;
                break;
            }
            catch (SocketException)
            {
                // Port is likely in use, try the next one
            }
        }

        if (!bound)
        {
            // Fallback to random ephemeral port if the range is exhausted or invalid
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            Port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
        }

        _dataServer = new NetServer();
        _dataServer.OnClientConnected += c =>
        {
            if (!AreIpAddressesEqual(c.Ip, authorizedClientIp))
            {
                // Unblock GetStreamAsync with an exception to prevent hanging the control connection
                _streamTcs.TrySetException(new UnauthorizedAccessException($"Data connection from {c.Ip} rejected. Expected {authorizedClientIp}."));
                
                // Stop the client asynchronously to prevent potential library crashes
                Task.Run(() => c.Stop());
                return;
            }
            _stream = new MikuClientStream(c);
            _streamTcs.TrySetResult(_stream);
        };
        _dataServer.OnClientDataReceived += (c, data) =>
        {
            if (AreIpAddressesEqual(c.Ip, authorizedClientIp))
            {
                _stream?.EnqueueData(data);
            }
        };
        _dataServer.OnClientDisconnected += (c, reason) =>
        {
            _stream?.Complete();
        };
        _dataServer.Start(ip, Port);
    }

    private static bool AreIpAddressesEqual(string ip1, string ip2)
    {
        if (ip1 == ip2) return true;
        if (string.IsNullOrEmpty(ip1) || string.IsNullOrEmpty(ip2)) return false;

        if (IPAddress.TryParse(ip1, out var addr1) && IPAddress.TryParse(ip2, out var addr2))
        {
            if (addr1.Equals(addr2)) return true;

            // Normalize to IPv6 for comparison to handle IPv4-mapped IPv6 addresses
            return addr1.MapToIPv6().Equals(addr2.MapToIPv6());
        }

        return false;
    }

    /// <inheritdoc/>
    public Task<Stream> GetStreamAsync() => _streamTcs.Task;

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        _dataServer?.Stop();
        _dataServer = null;
        return ValueTask.CompletedTask;
    }
}
