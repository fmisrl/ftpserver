using System.ComponentModel.DataAnnotations;

namespace FmiSrl.FtpServer.Server;

/// <summary>
/// Provides configuration options for the FTP server.
/// </summary>
public class FtpServerConfigurationOptions
{
    /// <summary>
    /// Gets or sets the IP address the FTP server listens on.
    /// </summary>
    /// <value>The listening IP address as a <see cref="string"/>. Defaults to "0.0.0.0".</value>
    [Required]
    public string ListeningIp { get; set; } = "0.0.0.0";

    /// <summary>
    /// Gets or sets the port number the FTP server listens on for control connections.
    /// </summary>
    /// <value>The FTP port as an <see cref="int"/>. Defaults to 21.</value>
    [Range(1, 65535)]
    public int FtpPort { get; set; } = 21;

    /// <summary>
    /// Gets or sets the minimum port number in the range for passive data connections.
    /// </summary>
    /// <value>The minimum passive port as an <see cref="int"/>. Defaults to 50000.</value>
    [Range(1, 65535)]
    public int PasvMinPort { get; set; } = 50000;

    /// <summary>
    /// Gets or sets the maximum port number in the range for passive data connections.
    /// </summary>
    /// <value>The maximum passive port as an <see cref="int"/>. Defaults to 50100.</value>
    [Range(1, 65535)]
    public int PasvMaxPort { get; set; } = 50100;

    /// <summary>
    /// Gets or sets the name of the FTP server presented in the welcome message.
    /// </summary>
    /// <value>The server name as a <see cref="string"/>. Defaults to "FMI FTP Server".</value>
    public string ServerName { get; set; } = "FMI FTP Server";
}
