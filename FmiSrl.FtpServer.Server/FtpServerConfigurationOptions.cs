using System.ComponentModel.DataAnnotations;

namespace FmiSrl.FtpServer.Server;

public class FtpServerConfigurationOptions
{
    [Required]
    public string ListeningIp { get; set; } = "0.0.0.0";

    [Range(1, 65535)]
    public int FtpPort { get; set; } = 21;

    [Range(1, 65535)]
    public int PasvMinPort { get; set; } = 50000;

    [Range(1, 65535)]
    public int PasvMaxPort { get; set; } = 50100;

    public string ServerName { get; set; } = "FMI FTP Server";
}
