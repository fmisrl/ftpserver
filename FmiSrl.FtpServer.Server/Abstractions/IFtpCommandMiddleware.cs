namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Defines the behavior for an FTP command middleware.
/// </summary>
public interface IFtpCommandMiddleware
{
    /// <summary>
    /// Invokes the middleware asynchronously.
    /// </summary>
    /// <param name="context">The <see cref="FtpCommandContext"/> for the current command.</param>
    /// <param name="next">A delegate representing the next step in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InvokeAsync(FtpCommandContext context, Func<Task> next);
}
