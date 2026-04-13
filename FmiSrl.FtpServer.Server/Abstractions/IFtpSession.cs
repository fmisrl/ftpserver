using System.Net;

namespace FmiSrl.FtpServer.Server.Abstractions;

public interface IFtpSession
{
    string Id { get; }
    bool IsAuthenticated { get; set; }
    string? Username { get; set; }
    string CurrentDirectory { get; set; }
    EndPoint RemoteEndPoint { get; }
    
    // Data connection state
    IFtpDataConnection? DataConnection { get; set; }
    
    Task SendResponseAsync(int code, string message);
    Task SendResponseAsync(string rawResponse);

    Task<IDisposable> LockSessionAsync();
}

public interface IFtpDataConnection : IAsyncDisposable
{
    Task<Stream> GetStreamAsync();
}
