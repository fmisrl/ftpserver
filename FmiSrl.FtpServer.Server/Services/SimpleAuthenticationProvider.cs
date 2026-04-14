using FmiSrl.FtpServer.Server.Abstractions;
using Microsoft.Extensions.Options;

namespace FmiSrl.FtpServer.Server.Services;

public class SimpleAuthenticationProvider(IOptions<SimpleAuthenticationProviderOptions> options) : IAuthenticationProvider
{
    private readonly SimpleAuthenticationProviderOptions _options = options.Value;

    public Task<bool> AuthenticateAsync(string username, string password)
    {
        return Task.FromResult(username == _options.Username && password == _options.Password);
    }
}
