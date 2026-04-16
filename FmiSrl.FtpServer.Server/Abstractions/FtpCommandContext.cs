using Microsoft.Extensions.Logging;

namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Provides contextual information for the execution of an FTP command.
/// </summary>
/// <param name="Session">The FTP session executing the command.</param>
/// <param name="Verb">The command verb (e.g., "USER", "PASS").</param>
/// <param name="Arguments">The arguments provided with the command.</param>
/// <param name="FileSystem">The file system provider to use.</param>
/// <param name="Authenticator">The authentication provider to use.</param>
/// <param name="Configuration">The server configuration options.</param>
/// <param name="Logger">The logger to use for recording command execution details.</param>
public record FtpCommandContext(
    IFtpSession Session,
    string Verb,
    string Arguments,
    IFileSystemProvider FileSystem,
    IAuthenticationProvider Authenticator,
    FtpServerConfigurationOptions Configuration,
    ILogger Logger
)
{
    /// <summary>
    /// Gets the authentication context for the current session.
    /// </summary>
    /// <value>A <see cref="FtpAuthenticationContext"/> for the current session.</value>
    public FtpAuthenticationContext AuthContext => new(Session.Username);
}
