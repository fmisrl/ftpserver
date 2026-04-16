namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Defines the behavior for an FTP command.
/// </summary>
public interface IFtpCommand
{
    /// <summary>
    /// Gets the collection of verbs supported by this command.
    /// </summary>
    /// <value>An array of <see cref="string"/> representing the supported verbs.</value>
    string[] Verbs { get; }

    /// <summary>
    /// Gets a value indicating whether this command requires the user to be authenticated.
    /// </summary>
    /// <value><c>true</c> if authentication is required; otherwise, <c>false</c>.</value>
    bool RequiresAuthentication { get; }

    /// <summary>
    /// Executes the FTP command asynchronously with the specified context.
    /// </summary>
    /// <param name="context">The <see cref="FtpCommandContext"/> for the command execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(FtpCommandContext context);
}
