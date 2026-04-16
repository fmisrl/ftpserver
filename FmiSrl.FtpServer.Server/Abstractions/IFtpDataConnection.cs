namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Defines the behavior for an FTP data connection.
/// </summary>
public interface IFtpDataConnection : IAsyncDisposable
{
    /// <summary>
    /// Gets the stream used for data transfer operations.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Stream"/> for the data connection.</returns>
    Task<Stream> GetStreamAsync();
}
