using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class CwdCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public CwdCommandTests()
    {
        _context = new FtpCommandContext(_session, "CWD", "dir", _fileSystem, _authenticator, new(), NullLogger.Instance);
        _session.CurrentDirectory.Returns("/");
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new CwdCommand();
        Assert.Equal(["CWD"], sut.Verbs);
        Assert.True(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArgsEmpty_ShouldReturn501()
    {
        var sut = new CwdCommand();
        await sut.ExecuteAsync(_context with { Arguments = "" });
        await _session.Received().SendResponseAsync(501, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDirectoryExists_ShouldChangeDirectory()
    {
        var sut = new CwdCommand();
        _fileSystem.DirectoryExistsAsync(Arg.Any<FtpAuthenticationContext>(), "/dir").Returns(true);
        await sut.ExecuteAsync(_context);
        _session.Received().CurrentDirectory = "/dir";
        await _session.Received().SendResponseAsync(250, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDirectoryDoesNotExist_ShouldReturn550()
    {
        var sut = new CwdCommand();
        _fileSystem.DirectoryExistsAsync(Arg.Any<FtpAuthenticationContext>(), "/dir").Returns(false);
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(550, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenException_ShouldReturn550()
    {
        var sut = new CwdCommand();
        _fileSystem.DirectoryExistsAsync(Arg.Any<FtpAuthenticationContext>(), "/dir").Throws(new InvalidOperationException());
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(550, Arg.Any<string>());
    }
}
