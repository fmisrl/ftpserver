using System.Net;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class PasvCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public PasvCommandTests()
    {
        _context = new FtpCommandContext(_session, "PASV", "", _fileSystem, _authenticator, new(), NullLogger.Instance);
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new PasvCommand();
        Assert.Equal(["PASV"], sut.Verbs);
        Assert.True(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetDataConnectionAndReturn227()
    {
        var sut = new PasvCommand();
        _session.RemoteEndPoint.Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
        await sut.ExecuteAsync(_context);
        Assert.NotNull(_session.DataConnection);
        await _session.Received().SendResponseAsync(227, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOldDataConnectionExists_ShouldDisposeOldConnection()
    {
        var sut = new PasvCommand();
        var oldConn = Substitute.For<IFtpDataConnection>();
        _session.DataConnection = oldConn;
        _session.RemoteEndPoint.Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));

        await sut.ExecuteAsync(_context);

        await oldConn.Received().DisposeAsync();
        Assert.NotSame(oldConn, _session.DataConnection);
        await _session.Received().SendResponseAsync(227, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonLoopbackRemoteIp_ShouldResolveLocalIp()
    {
        var sut = new PasvCommand();
        _session.RemoteEndPoint.Returns(new IPEndPoint(IPAddress.Parse("8.8.8.8"), 1234));
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(227, Arg.Any<string>());
    }
}
