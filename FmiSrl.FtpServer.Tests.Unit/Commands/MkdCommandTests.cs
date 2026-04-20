using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class MkdCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public MkdCommandTests()
    {
        _context = new FtpCommandContext(_session, "MKD", "newdir", _fileSystem, _authenticator, new(), NullLogger.Instance);
        _session.CurrentDirectory.Returns("/");
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new MkdCommand();
        Assert.Equal(["MKD"], sut.Verbs);
        Assert.True(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArgsEmpty_ShouldReturn501()
    {
        var sut = new MkdCommand();
        await sut.ExecuteAsync(_context with { Arguments = "" });
        await _session.Received().SendResponseAsync(501, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDirectoryExists_ShouldReturn550()
    {
        var sut = new MkdCommand();
        _fileSystem.DirectoryExistsAsync(Arg.Any<FtpAuthenticationContext>(), "/newdir").Returns(true);
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(550, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDirectoryDoesNotExist_ShouldCreateAndReturn257()
    {
        var sut = new MkdCommand();
        _fileSystem.DirectoryExistsAsync(Arg.Any<FtpAuthenticationContext>(), "/newdir").Returns(false);
        await sut.ExecuteAsync(_context);
        await _fileSystem.Received().CreateDirectoryAsync(Arg.Any<FtpAuthenticationContext>(), "/newdir");
        await _session.Received().SendResponseAsync(257, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenException_ShouldReturn550()
    {
        var sut = new MkdCommand();
        _fileSystem.DirectoryExistsAsync(Arg.Any<FtpAuthenticationContext>(), "/newdir").Throws(new InvalidOperationException());
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(550, Arg.Any<string>());
    }
}
