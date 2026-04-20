using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FmiSrl.FtpServer.Tests.Unit.Infrastructure;

public class FtpCommandHandlerTests
{
    private readonly FtpCommandHandler _sut = new();
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();

    [Fact]
    public async Task When_command_is_not_registered_should_return_502()
    {
        // Arrange
        var context = new FtpCommandContext(
            _session,
            "UNKNOWN",
            "",
            _fileSystem,
            _authenticator,
            new(),
            NullLogger.Instance
        );

        // Act
        await _sut.HandleCommandAsync(context);

        // Assert
        await _session.Received().SendResponseAsync(502, Arg.Any<string>());
    }

    [Fact]
    public async Task When_command_requires_auth_and_session_is_not_authenticated_should_return_530()
    {
        // Arrange
        var command = Substitute.For<IFtpCommand>();
        command.Verbs.Returns(["AUTH_REQ"]);
        command.RequiresAuthentication.Returns(true);
        _sut.RegisterCommand(command);

        _session.IsAuthenticated.Returns(false);

        var context = new FtpCommandContext(
            _session,
            "AUTH_REQ",
            "",
            _fileSystem,
            _authenticator,
            new(),
            NullLogger.Instance
        );

        // Act
        await _sut.HandleCommandAsync(context);

        // Assert
        await _session.Received().SendResponseAsync(530, Arg.Any<string>());
        await command.DidNotReceive().ExecuteAsync(Arg.Any<FtpCommandContext>());
    }

    [Fact]
    public async Task When_command_is_registered_and_auth_ok_should_execute_command()
    {
        // Arrange
        var command = Substitute.For<IFtpCommand>();
        command.Verbs.Returns(["TEST"]);
        command.RequiresAuthentication.Returns(false);
        _sut.RegisterCommand(command);

        var context = new FtpCommandContext(
            _session,
            "TEST",
            "args",
            _fileSystem,
            _authenticator,
            new(),
            NullLogger.Instance
        );

        // Act
        await _sut.HandleCommandAsync(context);

        // Assert
        await command.Received().ExecuteAsync(context);
    }
}
