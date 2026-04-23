using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class SizeCommandTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public SizeCommandTests()
    {
        _context = new FtpCommandContext(_session, "SIZE", "file.txt", _fileSystem, _authenticator, new(), NullLogger.Instance);
        _session.CurrentDirectory.Returns("/");
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new SizeCommand();
        Assert.Equal(["SIZE"], sut.Verbs);
        Assert.True(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArgsEmpty_ShouldReturn501()
    {
        var sut = new SizeCommand();
        await sut.ExecuteAsync(_context with { Arguments = "" });
        await _session.Received().SendResponseAsync(501, Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFileExists_ShouldReturnSize()
    {
        var sut = new SizeCommand();
        var entry = new FileSystemEntry("file.txt", 1234, DateTime.Now, false);
        _fileSystem.GetEntryAsync(Arg.Any<FtpAuthenticationContext>(), "/file.txt").Returns(entry);
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(213, "1234");
    }

    [Fact]
    public async Task ExecuteAsync_WhenFileNotFound_ShouldReturn550()
    {
        var sut = new SizeCommand();
        _fileSystem.GetEntryAsync(Arg.Any<FtpAuthenticationContext>(), "/file.txt").Returns((FileSystemEntry?)null);
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(550, "File not found or is a directory.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenException_ShouldReturn550()
    {
        var sut = new SizeCommand();
        _fileSystem.GetEntryAsync(Arg.Any<FtpAuthenticationContext>(), "/file.txt").Throws(new InvalidOperationException());
        await sut.ExecuteAsync(_context);
        await _session.Received().SendResponseAsync(550, "Error getting file size.");
    }
}
