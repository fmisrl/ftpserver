namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Defines the behavior for an authentication provider used by the FTP server.
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>
    /// Authenticates a user with the specified username and password asynchronously.
    /// </summary>
    /// <param name="username">The username to authenticate.</param>
    /// <param name="password">The password to authenticate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <c>true</c> if the authentication is successful; otherwise, <c>false</c>.</returns>
    Task<bool> AuthenticateAsync(string username, string password);
}
