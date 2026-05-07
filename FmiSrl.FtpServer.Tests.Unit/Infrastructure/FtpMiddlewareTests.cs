using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace FmiSrl.FtpServer.Tests.Unit.Infrastructure;

public class FtpMiddlewareTests
{
    private readonly IFtpSession _session = Substitute.For<IFtpSession>();
    private readonly IFileSystemProvider _fileSystem = Substitute.For<IFileSystemProvider>();
    private readonly IAuthenticationProvider _authenticator = Substitute.For<IAuthenticationProvider>();

    [Fact]
    public async Task Middleware_should_be_able_to_manipulate_response()
    {
        // Arrange
        var middleware = new TestManipulationMiddleware();
        var sut = new FtpCommandHandler(new[] { middleware });
        
        var command = Substitute.For<IFtpCommand>();
        command.Verbs.Returns(["TEST"]);
        command.ExecuteAsync(Arg.Any<FtpCommandContext>()).Returns(async call =>
        {
            var ctx = call.Arg<FtpCommandContext>();
            await ctx.Session.SendResponseAsync(200, "Original Message");
        });
        sut.RegisterCommand(command);

        var context = new FtpCommandContext(
            _session,
            "TEST",
            "",
            _fileSystem,
            _authenticator,
            new(),
            NullLogger.Instance
        );

        // Act
        await sut.HandleCommandAsync(context);

        // Assert
        await _session.Received().SendResponseAsync(201, "Manipulated Message");
    }

    private class TestManipulationMiddleware : IFtpCommandMiddleware
    {
        public async Task InvokeAsync(FtpCommandContext context, Func<Task> next)
        {
            await next();
            
            if (context.Response != null)
            {
                context.Response.Code = 201;
                context.Response.Message = "Manipulated Message";
            }
        }
    }
}
