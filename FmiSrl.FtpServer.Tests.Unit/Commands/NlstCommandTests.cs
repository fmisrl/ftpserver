using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class NlstCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public NlstCommandTests()
    {
        _session.CurrentDirectory.Returns("/");
        _context = new FtpCommandContext(_session, "NLST", "", _fileSystem, _authenticator, new(), NullLogger.Instance);
        _session.Username.Returns("test");
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new NlstCommand();
        Assert.Equal(["NLST"], sut.Verbs);
        Assert.True(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoDataConnection_ShouldReturn425()
    {
        var sut = new NlstCommand();
        _session.DataConnection.Returns((IFtpDataConnection?)null);
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(425, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccess_ShouldReturn150And226()
    {
        var sut = new NlstCommand();
        var dataConn = Substitute.For<IFtpDataConnection>();
        dataConn.GetStreamAsync().Returns(new MemoryStream());
        _session.DataConnection.Returns(dataConn);

        var entries = new[] { new FileSystemEntry("file.txt", 100, DateTime.Now, false) };
        _fileSystem.GetEntriesAsync(Arg.Any<FtpAuthenticationContext>(), "/").Returns(entries);

        await sut.ExecuteAsync(_context);

        await _session.Received().SendResponseAsync(150, Arg.Any<string>());
        await _session.Received().SendResponseAsync(226, Arg.Any<string>());
        await dataConn.Received().DisposeAsync();
    }
}
