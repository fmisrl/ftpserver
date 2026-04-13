namespace FmiSrl.FtpServer.Server;

public class FtpServerConfigurationOptions
{
    public string ListeningIp { get; set; } = "0.0.0.0";
    public int FtpPort { get; set; } = 21;
    public int PasvMinPort { get; set; } = 50000;
    public int PasvMaxPort { get; set; } = 50100;
    public string ServerName { get; set; } = "FMI FTP Server";
}
