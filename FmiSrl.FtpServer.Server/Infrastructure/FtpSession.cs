using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using FmiSrl.FtpServer.Server.Abstractions;
using Miku.Core;

namespace FmiSrl.FtpServer.Server.Infrastructure;

public class FtpSession : IFtpSession
{
    private readonly NetClient _client;
    private readonly SemaphoreSlim _commandSemaphore = new(1, 1);

    public FtpSession(NetClient client)
    {
        _client = client;
        Id = Guid.NewGuid().ToString();
        CurrentDirectory = "/";
    }

    public string Id { get; }
    public bool IsAuthenticated { get; set; }
    public string? Username { get; set; }
    public string CurrentDirectory { get; set; }
    
    public EndPoint RemoteEndPoint => new IPEndPoint(IPAddress.Parse(_client.Ip), 0); // Placeholder port

    public IFtpDataConnection? DataConnection { get; set; }

    public StringBuilder CommandBuffer { get; } = new StringBuilder();

    public async Task SendResponseAsync(int code, string message)
    {
        await SendResponseAsync($"{code} {message}\r\n");
    }

    public Task SendResponseAsync(string rawResponse)
    {
        byte[] data = Encoding.UTF8.GetBytes(rawResponse);
        _client.Send(data);
        return Task.CompletedTask;
    }

    public async Task<IDisposable> LockSessionAsync()
    {
        await _commandSemaphore.WaitAsync();
        return new SessionLock(_commandSemaphore);
    }

    private class SessionLock : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public SessionLock(SemaphoreSlim semaphore) => _semaphore = semaphore;
        public void Dispose() => _semaphore.Release();
    }
}

public class MikuClientStream : Stream
{
    private readonly NetClient _client;
    private readonly Channel<byte[]> _readChannel = Channel.CreateUnbounded<byte[]>();
    private byte[]? _currentReadBuffer;
    private int _currentReadBufferPosition;
    private bool _isDisposed;

    public MikuClientStream(NetClient client)
    {
        _client = client;
    }

    public void EnqueueData(ReadOnlyMemory<byte> data)
    {
        if (data.Length == 0)
        {
            _readChannel.Writer.TryComplete();
            return;
        }
        _readChannel.Writer.TryWrite(data.ToArray());
    }

    public void Complete()
    {
        _readChannel.Writer.TryComplete();
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush() 
    {
        // Simple wait for library to finish background tasks
        int timeout = 0;
        while (_client.HasPendingSends && timeout < 200) // 10 seconds max
        {
            Thread.Sleep(50);
            timeout++;
        }
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        int timeout = 0;
        while (_client.HasPendingSends && timeout < 200)
        {
            await Task.Delay(50, cancellationToken);
            timeout++;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_currentReadBuffer == null || _currentReadBufferPosition >= _currentReadBuffer.Length)
        {
            try
            {
                if (await _readChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    if (_readChannel.Reader.TryRead(out var chunk))
                    {
                        _currentReadBuffer = chunk;
                        _currentReadBufferPosition = 0;
                    }
                }
                else
                {
                    return 0; // EOF
                }
            }
            catch (ChannelClosedException)
            {
                return 0;
            }
        }

        if (_currentReadBuffer != null && _currentReadBufferPosition < _currentReadBuffer.Length)
        {
            int available = _currentReadBuffer.Length - _currentReadBufferPosition;
            int toCopy = Math.Min(available, count);
            Buffer.BlockCopy(_currentReadBuffer, _currentReadBufferPosition, buffer, offset, toCopy);
            _currentReadBufferPosition += toCopy;
            return toCopy;
        }

        return 0;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (count == 0) return;
        
        // Blocking wait for backpressure
        int backoff = 0;
        while (_client.HasPendingSends && backoff < 100)
        {
            Thread.Sleep(10);
            backoff++;
        }

        byte[] data = new byte[count];
        Buffer.BlockCopy(buffer, offset, data, 0, count);
        _client.Send(data);
    }
    
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (count == 0) return;

        // Break large writes into smaller chunks to help Miku's internal buffers
        const int chunkSize = 16384;
        int processed = 0;
        while (processed < count)
        {
            int toSend = Math.Min(chunkSize, count - processed);
            
            // Wait for backpressure
            int backoff = 0;
            while (_client.HasPendingSends && backoff < 50)
            {
                await Task.Delay(10, cancellationToken);
                backoff++;
            }

            byte[] data = new byte[toSend];
            Buffer.BlockCopy(buffer, offset + processed, data, 0, toSend);
            _client.Send(data);
            processed += toSend;
        }
    }
    
    public override async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Ensure everything is sent before we kill the client
        await FlushAsync(default);
        await Task.Delay(500); 
        
        try { _client.Stop(); } catch { }
        await base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed && disposing)
        {
            _isDisposed = true;
            Flush();
            Thread.Sleep(500);
            try { _client.Stop(); } catch { }
        }
        base.Dispose(disposing);
    }
}

public class MikuPassiveDataConnection : IFtpDataConnection
{
    private NetServer? _dataServer;
    private MikuClientStream? _stream;
    private readonly TaskCompletionSource<Stream> _streamTcs = new();

    public int Port { get; }

    public MikuPassiveDataConnection(string ip, int minPort, int maxPort)
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
            _stream = new MikuClientStream(c);
            _streamTcs.TrySetResult(_stream);
        };
        _dataServer.OnClientDataReceived += (c, data) => {
            _stream?.EnqueueData(data);
        };
        _dataServer.OnClientDisconnected += (c, reason) => {
            _stream?.Complete();
        };
        _dataServer.Start(ip, Port);
    }

    public Task<Stream> GetStreamAsync()
    {
        return _streamTcs.Task;
    }

    public ValueTask DisposeAsync()
    {
        _dataServer?.Stop();
        _dataServer = null;
        return ValueTask.CompletedTask;
    }
}
