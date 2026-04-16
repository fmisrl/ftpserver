using System.Threading.Channels;
using Miku.Core;

namespace FmiSrl.FtpServer.Server.Infrastructure;

/// <summary>
/// Provides a stream implementation that wraps a <see cref="NetClient"/> for reading and writing data.
/// </summary>
public class MikuClientStream : Stream
{
    private readonly NetClient _client;
    private readonly Channel<byte[]> _readChannel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(128)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = true
    });
    private byte[]? _currentReadBuffer;
    private int _currentReadBufferPosition;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MikuClientStream"/> class.
    /// </summary>
    /// <param name="client">The <see cref="NetClient"/> to wrap.</param>
    public MikuClientStream(NetClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Enqueues data received from the client for reading from the stream.
    /// </summary>
    /// <param name="data">The data to enqueue.</param>
    public void EnqueueData(ReadOnlyMemory<byte> data)
    {
        if (data.Length == 0)
        {
            _readChannel.Writer.TryComplete();
            return;
        }
        _readChannel.Writer.TryWrite(data.ToArray());
    }

    /// <summary>
    /// Completes the reading channel, indicating no more data will be received.
    /// </summary>
    public void Complete()
    {
        _readChannel.Writer.TryComplete();
    }

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => true;

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    /// <inheritdoc/>
    public override void Flush() 
    {
        // Simple wait for library to finish background tasks
        var timeout = 0;
        while (_client.HasPendingSends && timeout < 200) // 10 seconds max
        {
            Thread.Sleep(50);
            timeout++;
        }
    }

    /// <inheritdoc/>
    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        var timeout = 0;
        while (_client.HasPendingSends && timeout < 200)
        {
            await Task.Delay(50, cancellationToken);
            timeout++;
        }
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
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
            var available = _currentReadBuffer.Length - _currentReadBufferPosition;
            var toCopy = Math.Min(available, buffer.Length);
            _currentReadBuffer.AsSpan(_currentReadBufferPosition, toCopy).CopyTo(buffer.Span);
            _currentReadBufferPosition += toCopy;
            return toCopy;
        }

        return 0;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (count == 0) return;
        
        // Blocking wait for backpressure
        var backoff = 0;
        while (_client.HasPendingSends && backoff < 100)
        {
            Thread.Sleep(10);
            backoff++;
        }

        var data = new byte[count];
        Buffer.BlockCopy(buffer, offset, data, 0, count);
        _client.Send(data);
    }
    
    /// <inheritdoc/>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.Length == 0) return;

        // Break large writes into smaller chunks to help Miku's internal buffers
        const int chunkSize = 16384;
        var processed = 0;
        while (processed < buffer.Length)
        {
            var toSend = Math.Min(chunkSize, buffer.Length - processed);
            
            // Wait for backpressure
            var backoff = 0;
            while (_client.HasPendingSends && backoff < 50)
            {
                await Task.Delay(10, cancellationToken);
                backoff++;
            }

            var data = buffer.Slice(processed, toSend).ToArray();
            _client.Send(data);
            processed += toSend;
        }
    }
    
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
