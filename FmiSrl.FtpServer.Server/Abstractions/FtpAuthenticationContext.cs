namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Provides contextual information for FTP authentication.
/// </summary>
/// <param name="Username">The username associated with the authentication context.</param>
public record FtpAuthenticationContext(string? Username);
