namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Defines event handlers for FTP server events.
/// </summary>
public interface IFtpServerEventHandler
{
    /// <summary>
    /// Invoked when a file is successfully uploaded to the server.
    /// </summary>
    /// <param name="session">The FTP session that uploaded the file.</param>
    /// <param name="targetFile">The full path of the uploaded file.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task OnFileUploadedAsync(IFtpSession session, string targetFile);
}
