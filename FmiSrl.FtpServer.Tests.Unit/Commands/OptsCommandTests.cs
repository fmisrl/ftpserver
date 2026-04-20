using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class OptsCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public OptsCommandTests()
    {
        _context = new FtpCommandContext(_session, "OPTS", "UTF8 ON", _fileSystem, _authenticator, new(), NullLogger.Instance);
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new OptsCommand();
        Assert.Equal(["OPTS"], sut.Verbs);
        Assert.False(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUtf8On_ShouldReturn200()
    {
        var sut = new OptsCommand();
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(200, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOther_ShouldReturn501()
    {
        var sut = new OptsCommand();
        await sut.ExecuteAsync(_context with { Arguments = "SOME OTHER" });
        await _session.Received().SendResponseAsync(501, Arg.Any<string>());
    }
}
