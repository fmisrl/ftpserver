using FmiSrl.FtpServer.Server.Abstractions;
using Microsoft.Extensions.Options;

namespace FmiSrl.FtpServer.Server.Services;

/// <summary>
/// Provides a simple implementation of <see cref="IAuthenticationProvider"/> that checks against a single username and password.
/// </summary>
public class SimpleAuthenticationProvider(IOptions<SimpleAuthenticationProviderOptions> options)
    : IAuthenticationProvider
{
    private readonly SimpleAuthenticationProviderOptions _options = options.Value;

    /// <inheritdoc/>
    public Task<bool> AuthenticateAsync(string username, string password) =>
        Task.FromResult(username == _options.Username && password == _options.Password);
}
