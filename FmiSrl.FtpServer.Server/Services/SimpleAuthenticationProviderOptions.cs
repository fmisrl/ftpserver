namespace FmiSrl.FtpServer.Server.Services;

/// <summary>
/// Provides configuration options for the simple authentication provider.
/// </summary>
public class SimpleAuthenticationProviderOptions
{
    /// <summary>
    /// Gets or sets the required username for authentication.
    /// </summary>
    /// <value>The username as a <see cref="string"/>.</value>
    public string Username { get; set; } = "admin";

    /// <summary>
    /// Gets or sets the required password for authentication.
    /// </summary>
    /// <value>The password as a <see cref="string"/>.</value>
    public string Password { get; set; } = "password";
}
