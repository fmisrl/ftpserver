namespace FmiSrl.FtpServer.Server.Abstractions;

public interface IAuthenticationProvider
{
    Task<bool> AuthenticateAsync(string username, string password);
}
