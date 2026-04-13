using Microsoft.Extensions.Logging;

namespace FmiSrl.FtpServer.Server.Abstractions;

public interface IFtpCommand
{
    string[] Verbs { get; }
    Task ExecuteAsync(FtpCommandContext context);
}

public record FtpCommandContext(
    IFtpSession Session,
    string Verb,
    string Arguments,
    IFileSystemProvider FileSystem,
    IAuthenticationProvider Authenticator,
    FtpServerConfigurationOptions Configuration,
    ILogger Logger
);
