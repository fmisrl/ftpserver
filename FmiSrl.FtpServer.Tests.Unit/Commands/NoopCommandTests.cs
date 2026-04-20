using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class NoopCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public NoopCommandTests()
    {
        _context = new FtpCommandContext(_session, "NOOP", "", _fileSystem, _authenticator, new(), NullLogger.Instance);
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new NoopCommand();
        Assert.Equal(["NOOP"], sut.Verbs);
        Assert.False(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturn200()
    {
        var sut = new NoopCommand();
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(200, Arg.Any<string>());
    }
}
