using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class PassCommandTests
{
    private readonly PassCommand _sut = new();
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();

    [Fact]
    public async Task When_auth_is_successful_should_set_session_authenticated_and_return_230()
    {
        // Arrange
        _session.Username.Returns("testuser");
        _authenticator.AuthenticateAsync("testuser", "password").Returns(true);

        var context = new FtpCommandContext(
            _session,
            "PASS",
            "password",
            _fileSystem,
            _authenticator,
            new FmiSrl.FtpServer.Server.FtpServerConfigurationOptions(),
            NullLogger.Instance
        );

        // Act
        await _sut.ExecuteAsync(context);

        // Assert
        _session.Received().IsAuthenticated = true;
        await _session.Received().SendResponseAsync(230, Arg.Any<string>());
    }

    [Fact]
    public async Task When_auth_fails_should_return_530()
    {
        // Arrange
        _session.Username.Returns("testuser");
        _authenticator.AuthenticateAsync("testuser", "wrongpassword").Returns(false);

        var context = new FtpCommandContext(
            _session,
            "PASS",
            "wrongpassword",
            _fileSystem,
            _authenticator,
            new(),
            NullLogger.Instance
        );

        // Act
        await _sut.ExecuteAsync(context);

        // Assert
        _session.DidNotReceive().IsAuthenticated = true;
        await _session.Received().SendResponseAsync(530, Arg.Any<string>());
    }

    [Fact]
    public async Task When_user_not_sent_should_return_503()
    {
        // Arrange
        _session.Username.Returns((string?)null);

        var context = new FtpCommandContext(
            _session,
            "PASS",
            "password",
            _fileSystem,
            _authenticator,
            new(),
            NullLogger.Instance
        );

        // Act
        await _sut.ExecuteAsync(context);

        // Assert
        await _session.Received().SendResponseAsync(503, Arg.Any<string>());
    }
}
