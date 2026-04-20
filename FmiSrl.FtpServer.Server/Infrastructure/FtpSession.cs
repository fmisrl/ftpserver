using System.Net;
using System.Text;
using FmiSrl.FtpServer.Server.Abstractions;
using Miku.Core;

namespace FmiSrl.FtpServer.Server.Infrastructure;

/// <summary>
/// Provides an implementation of <see cref="IFtpSession"/> that manages an FTP client session.
/// </summary>
public class FtpSession : IFtpSession
{
    private readonly NetClient _client;
    private readonly SemaphoreSlim _commandSemaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpSession"/> class.
    /// </summary>
    /// <param name="client">The <see cref="NetClient"/> associated with this session.</param>
    public FtpSession(NetClient client)
    {
        _client = client;
        Id = Guid.NewGuid().ToString();
        CurrentDirectory = "/";
    }

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public bool IsAuthenticated { get; set; }

    /// <inheritdoc/>
    public string? Username { get; set; }

    /// <inheritdoc/>
    public string CurrentDirectory { get; set; }

    /// <inheritdoc/>
    public EndPoint RemoteEndPoint => new IPEndPoint(IPAddress.Parse(_client.Ip), 0);// Placeholder port

    /// <inheritdoc/>
    public IFtpDataConnection? DataConnection { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, object> State { get; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the command buffer used for accumulating data from the client.
    /// </summary>
    /// <value>A <see cref="StringBuilder"/> representing the command buffer.</value>
    public StringBuilder CommandBuffer { get; } = new();

    /// <inheritdoc/>
    public async Task SendResponseAsync(int code, string message)
    {
        await SendResponseAsync($"{code} {message}\r\n");
    }

    /// <inheritdoc/>
    public Task SendResponseAsync(string rawResponse)
    {
        var data = Encoding.UTF8.GetBytes(rawResponse);
        _client.Send(data);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<IDisposable> LockSessionAsync()
    {
        await _commandSemaphore.WaitAsync();
        return new SessionLock(_commandSemaphore);
    }

    private sealed class SessionLock : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public SessionLock(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }
        public void Dispose() => _semaphore.Release();
    }
}
