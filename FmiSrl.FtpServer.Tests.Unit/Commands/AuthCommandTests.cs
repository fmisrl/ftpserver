using System.Net;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class AuthCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public AuthCommandTests()
    {
        _context = new FtpCommandContext(_session, "AUTH", "TLS", _fileSystem, _authenticator, new(), NullLogger.Instance);
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new AuthCommand();
        Assert.Equal(["AUTH"], sut.Verbs);
        Assert.False(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturn502()
    {
        var sut = new AuthCommand();
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(502, Arg.Any<string>());
    }
}
