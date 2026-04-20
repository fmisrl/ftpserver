using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class DeleCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public DeleCommandTests()
    {
        _context = new FtpCommandContext(_session, "DELE", "file.txt", _fileSystem, _authenticator, new(), NullLogger.Instance);
        _session.CurrentDirectory.Returns("/");
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new DeleCommand();
        Assert.Equal(["DELE"], sut.Verbs);
        Assert.True(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArgsEmpty_ShouldReturn501()
    {
        var sut = new DeleCommand();
        await sut.ExecuteAsync(_context with { Arguments = "" });
        await _session.Received().SendResponseAsync(501, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFileExists_ShouldDeleteAndReturn250()
    {
        var sut = new DeleCommand();
        _fileSystem.FileExistsAsync(Arg.Any<FtpAuthenticationContext>(), "/file.txt").Returns(true);
        await sut.ExecuteAsync(_context);
        await _fileSystem.Received().DeleteFileAsync(Arg.Any<FtpAuthenticationContext>(), "/file.txt");
        await _session.Received().SendResponseAsync(250, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFileDoesNotExist_ShouldReturn550()
    {
        var sut = new DeleCommand();
        _fileSystem.FileExistsAsync(Arg.Any<FtpAuthenticationContext>(), "/file.txt").Returns(false);
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(550, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenException_ShouldReturn550()
    {
        var sut = new DeleCommand();
        _fileSystem.FileExistsAsync(Arg.Any<FtpAuthenticationContext>(), "/file.txt").Returns(true);
        _fileSystem.DeleteFileAsync(Arg.Any<FtpAuthenticationContext>(), "/file.txt").Throws(new InvalidOperationException());
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(550, Arg.Any<string>());
    }
}
