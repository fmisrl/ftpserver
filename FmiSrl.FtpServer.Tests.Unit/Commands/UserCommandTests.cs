using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class UserCommandTests
{
    private readonly UserCommand _sut = new();
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();

    [Fact]
    public async Task When_username_is_provided_should_set_session_username_and_return_331()
    {
        // Arrange
        var context = new FtpCommandContext(
            _session,
            "USER",
            "testuser",
            _fileSystem,
            _authenticator,
            new(),
            NullLogger.Instance
        );

        // Act
        await _sut.ExecuteAsync(context);

        // Assert
        _session.Received().Username = "testuser";
        await _session.Received().SendResponseAsync(331, Arg.Is<string>(s => s.Contains("testuser")));
    }

    [Fact]
    public async Task When_username_is_missing_should_return_501()
    {
        // Arrange
        var context = new FtpCommandContext(
            _session,
            "USER",
            "",
            _fileSystem,
            _authenticator,
            new(),
            NullLogger.Instance
        );

        // Act
        await _sut.ExecuteAsync(context);

        // Assert
        await _session.Received().SendResponseAsync(501, Arg.Any<string>());
    }
}
