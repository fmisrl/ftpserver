using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FmiSrl.FtpServer.Tests.Unit.Commands;

public class RenameCommandsTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();
    private readonly FtpCommandContext _context;

    public RenameCommandsTests()
    {
        _context = new FtpCommandContext(_session, "RNFR", "old.txt", _fileSystem, _authenticator, new(), NullLogger.Instance);
        _session.CurrentDirectory.Returns("/");
        _session.State.Returns(new Dictionary<string, object>());
    }

    [Fact]
    public void Properties_ShouldBeCorrect()
    {
        var sut = new RenameCommands();
        Assert.Equal(["RNFR", "RNTO"], sut.Verbs);
        Assert.True(sut.RequiresAuthentication);
    }

    [Fact]
    public async Task RNFR_WhenArgsEmpty_ShouldReturn501()
    {
        var sut = new RenameCommands();
        await sut.ExecuteAsync(_context with { Arguments = "" });
        await _session.Received().SendResponseAsync(501, Arg.Any<string>());
    }

    [Fact]
    public async Task RNFR_WhenSuccess_ShouldUpdateStateAndReturn350()
    {
        var sut = new RenameCommands();
        await sut.ExecuteAsync(_context);
        Assert.True(_session.State.ContainsKey("RnfrPath"));
        Assert.Equal("/old.txt", _session.State["RnfrPath"]);
        await _session.Received().SendResponseAsync(350, Arg.Any<string>());
    }

    [Fact]
    public async Task RNTO_WhenNoRnfrState_ShouldReturn503()
    {
        var sut = new RenameCommands();
        await sut.ExecuteAsync(_context with { Verb = "RNTO", Arguments = "new.txt" });
        await _session.Received().SendResponseAsync(503, Arg.Any<string>());
    }

    [Fact]
    public async Task RNTO_WhenSuccess_ShouldRenameAndReturn250()
    {
        var sut = new RenameCommands();
        _session.State["RnfrPath"] = "/old.txt";
        await sut.ExecuteAsync(_context with { Verb = "RNTO", Arguments = "new.txt" });
        
        await _fileSystem.Received().RenameAsync(Arg.Any<FtpAuthenticationContext>(), "/old.txt", "/new.txt");
        await _session.Received().SendResponseAsync(250, Arg.Any<string>());
        Assert.False(_session.State.ContainsKey("RnfrPath"));
    }

    [Fact]
    public async Task RNTO_WhenException_ShouldReturn550()
    {
        var sut = new RenameCommands();
        _session.State["RnfrPath"] = "/old.txt";
        _fileSystem.RenameAsync(Arg.Any<FtpAuthenticationContext>(), "/old.txt", "/new.txt").Returns(Task.FromException(new InvalidOperationException()));

        await sut.ExecuteAsync(_context with { Verb = "RNTO", Arguments = "new.txt" });
        
        await _session.Received().SendResponseAsync(550, Arg.Any<string>());
        Assert.False(_session.State.ContainsKey("RnfrPath"));
    }
}
