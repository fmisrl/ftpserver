using System.Net;
using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Infrastructure;

/// <summary>
/// Handles the registration and execution of FTP commands.
/// </summary>
public class FtpCommandHandler(IEnumerable<IFtpCommandMiddleware> middlewares)
{
    private readonly Dictionary<string, IFtpCommand> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IFtpCommandMiddleware> _middlewares = middlewares.ToList();

    /// <summary>
    /// Registers an FTP command.
    /// </summary>
    /// <param name="command">The <see cref="IFtpCommand"/> to register.</param>
    public void RegisterCommand(IFtpCommand command)
    {
        foreach (var verb in command.Verbs)
        {
            _commands[verb] = command;
        }
    }

    /// <summary>
    /// Processes an FTP command context by executing the corresponding registered command.
    /// </summary>
    /// <param name="context">The <see cref="FtpCommandContext"/> for the command to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleCommandAsync(FtpCommandContext context)
    {
        var originalSession = context.Session;
        var capturingSession = new ResponseCapturingSession(originalSession, context);
        
        // Temporarily replace the session in the context to capture responses
        var contextWithCapturingSession = context with { Session = capturingSession };

        var next = async () =>
        {
            if (!_commands.TryGetValue(context.Verb, out var command))
            {
                context.Response = new FtpResponse(502, "Command not implemented.");
                return;
            }

            if (command.RequiresAuthentication && !context.Session.IsAuthenticated)
            {
                context.Response = new FtpResponse(530, "Not logged in.");
                return;
            }

            await command.ExecuteAsync(contextWithCapturingSession);
        };

        // Build the pipeline
        var pipeline = next;
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var currentNext = pipeline;
            pipeline = () => middleware.InvokeAsync(context, currentNext);
        }

        await pipeline();

        // If a response was captured or set, send it via the original session
        if (context.Response != null)
        {
            await originalSession.SendResponseAsync(context.Response.Code, context.Response.Message);
        }
    }

    private sealed class ResponseCapturingSession(IFtpSession inner, FtpCommandContext context) : IFtpSession
    {
        public string Id => inner.Id;
        public bool IsAuthenticated { get => inner.IsAuthenticated; set => inner.IsAuthenticated = value; }
        public string? Username { get => inner.Username; set => inner.Username = value; }
        public string CurrentDirectory { get => inner.CurrentDirectory; set => inner.CurrentDirectory = value; }
        public EndPoint RemoteEndPoint => inner.RemoteEndPoint;
        public IFtpDataConnection? DataConnection { get => inner.DataConnection; set => inner.DataConnection = value; }
        public IDictionary<string, object> State => inner.State;

        public Task SendResponseAsync(int code, string message)
        {
            if (code is >= 100 and < 200)
            {
                return inner.SendResponseAsync(code, message);
            }
            context.Response = new FtpResponse(code, message);
            return Task.CompletedTask;
        }

        public Task SendResponseAsync(string rawResponse)
        {
            // Try to parse raw response if it matches "XYZ Message"
            if (rawResponse.Length >= 4 && int.TryParse(rawResponse.AsSpan(0, 3), out var code) && rawResponse[3] == ' ')
            {
                if (code is >= 100 and < 200)
                {
                    return inner.SendResponseAsync(rawResponse);
                }
                context.Response = new FtpResponse(code, rawResponse[4..].TrimEnd('\r', '\n'));
                return Task.CompletedTask;
            }
            return inner.SendResponseAsync(rawResponse);
        }

        public Task<IDisposable> LockSessionAsync() => inner.LockSessionAsync();
    }
}
