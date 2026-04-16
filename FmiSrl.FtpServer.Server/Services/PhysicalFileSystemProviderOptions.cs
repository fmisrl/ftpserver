namespace FmiSrl.FtpServer.Server.Services;

/// <summary>
/// Provides configuration options for the physical file system provider.
/// </summary>
public class PhysicalFileSystemProviderOptions
{
    /// <summary>
    /// Gets or sets the root directory for the physical file system.
    /// </summary>
    /// <value>The root directory path as a <see cref="string"/>.</value>
    public string RootDirectory { get; set; } = "ftp_root";
}
