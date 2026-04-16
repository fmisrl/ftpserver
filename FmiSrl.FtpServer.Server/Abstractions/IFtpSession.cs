using System.Net;

namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Defines the behavior and state of an FTP session.
/// </summary>
public interface IFtpSession
{
    /// <summary>
    /// Gets the unique identifier for the session.
    /// </summary>
    /// <value>A <see cref="string"/> representing the session ID.</value>
    string Id { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the user in this session is authenticated.
    /// </summary>
    /// <value><c>true</c> if the user is authenticated; otherwise, <c>false</c>.</value>
    bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets the username associated with this session.
    /// </summary>
    /// <value>The username as a <see cref="string"/>, or <c>null</c> if not yet provided.</value>
    string? Username { get; set; }

    /// <summary>
    /// Gets or sets the current working directory for this session.
    /// </summary>
    /// <value>The current directory path as a <see cref="string"/>.</value>
    string CurrentDirectory { get; set; }

    /// <summary>
    /// Gets the remote endpoint of the client.
    /// </summary>
    /// <value>The <see cref="EndPoint"/> of the remote client.</value>
    EndPoint RemoteEndPoint { get; }
    
    /// <summary>
    /// Gets or sets the data connection for this session.
    /// </summary>
    /// <value>The <see cref="IFtpDataConnection"/> instance, or <c>null</c> if no data connection is active.</value>
    IFtpDataConnection? DataConnection { get; set; }
    
    /// <summary>
    /// Sends a response with a status code and message to the client asynchronously.
    /// </summary>
    /// <param name="code">The FTP response code.</param>
    /// <param name="message">The response message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendResponseAsync(int code, string message);

    /// <summary>
    /// Sends a raw response string to the client asynchronously.
    /// </summary>
    /// <param name="rawResponse">The raw response string to send.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendResponseAsync(string rawResponse);

    /// <summary>
    /// Acquires a lock on the session to ensure commands are processed sequentially.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IDisposable"/> that releases the lock when disposed.</returns>
    Task<IDisposable> LockSessionAsync();
}
