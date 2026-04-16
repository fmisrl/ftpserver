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
    public MikuPassiveDataConnection(string ip, int minPort, int maxPort, string authorizedClientIp)
    {
        // Try to find a free port in the specified range
        bool bound = false;

        for (int portToTry = minPort; portToTry <= maxPort; portToTry++)
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
        _dataServer.OnClientConnected += c => {
            if (c.Ip != authorizedClientIp)
            {
                c.Stop();
                return;
            }
            _stream = new MikuClientStream(c);
            _streamTcs.TrySetResult(_stream);
        };
        _dataServer.OnClientDataReceived += (c, data) => {
            if (c.Ip == authorizedClientIp)
            {
                _stream?.EnqueueData(data);
            }
        };
        _dataServer.OnClientDisconnected += (c, reason) => {
            _stream?.Complete();
        };
        _dataServer.Start(ip, Port);
    }

    /// <inheritdoc/>
    public Task<Stream> GetStreamAsync()
    {
        return _streamTcs.Task;
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        _dataServer?.Stop();
        _dataServer = null;
        return ValueTask.CompletedTask;
    }
}
