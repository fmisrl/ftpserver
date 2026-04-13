using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Services;

public class SimpleAuthenticationProvider : IAuthenticationProvider
{
    private readonly string _validUsername;
    private readonly string _validPassword;

    public SimpleAuthenticationProvider(string validUsername, string validPassword)
    {
        _validUsername = validUsername;
        _validPassword = validPassword;
    }

    public Task<bool> AuthenticateAsync(string username, string password)
    {
        return Task.FromResult(username == _validUsername && password == _validPassword);
    }
}
