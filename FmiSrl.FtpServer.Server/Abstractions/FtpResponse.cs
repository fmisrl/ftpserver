namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Represents an FTP response.
/// </summary>
public class FtpResponse
{
    /// <summary>
    /// Gets or sets the FTP response code.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpResponse"/> class.
    /// </summary>
    /// <param name="code">The FTP response code.</param>
    /// <param name="message">The response message.</param>
    public FtpResponse(int code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Code} {Message}";
}
