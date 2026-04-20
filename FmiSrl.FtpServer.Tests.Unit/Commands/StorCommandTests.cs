using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class StorCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public StorCommandTests()
    {
        _context = new FtpCommandContext(_session, "STOR", "file.txt", _fileSystem, _authenticator, new(), NullLogger.Instance);
        _session.CurrentDirectory.Returns("/");
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new StorCommand();
        Assert.Equal(["STOR"], sut.Verbs);
        Assert.True(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoDataConnection_ShouldReturn425()
    {
        var sut = new StorCommand();
        _session.DataConnection.Returns((IFtpDataConnection?)null);
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(425, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArgsEmpty_ShouldReturn501()
    {
        var sut = new StorCommand();
        _session.DataConnection.Returns(Substitute.For<IFtpDataConnection>());
        await sut.ExecuteAsync(_context with { Arguments = "" });
        await _session.Received().SendResponseAsync(501, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccess_ShouldReturn226()
    {
        var sut = new StorCommand();
        var dataConn = Substitute.For<IFtpDataConnection>();
        dataConn.GetStreamAsync().Returns(new MemoryStream());
        _session.DataConnection.Returns(dataConn);
        _fileSystem.OpenWriteAsync(Arg.Any<FtpAuthenticationContext>(), "/file.txt").Returns(new MemoryStream());

        await sut.ExecuteAsync(_context);

        await _session.Received().SendResponseAsync(150, Arg.Any<string>());
        await _session.Received().SendResponseAsync(226, Arg.Any<string>());
        await dataConn.Received().DisposeAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenException_ShouldReturn550()
    {
        var sut = new StorCommand();
        var dataConn = Substitute.For<IFtpDataConnection>();
        _session.DataConnection.Returns(dataConn);
        dataConn.GetStreamAsync().Throws(new InvalidOperationException());

        await sut.ExecuteAsync(_context);

        await _session.Received().SendResponseAsync(150, Arg.Any<string>());
        await _session.Received().SendResponseAsync(550, Arg.Any<string>());
        await dataConn.Received().DisposeAsync();
    }
}
