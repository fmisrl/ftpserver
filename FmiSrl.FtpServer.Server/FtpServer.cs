using System.Text;
using Miku.Core;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;
using FmiSrl.FtpServer.Server.Commands;
using FmiSrl.FtpServer.Server.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FmiSrl.FtpServer.Server;

/// <summary>
/// Represents an FTP server that handles file transfer operations
/// using the File Transfer Protocol (FTP).
/// </summary>
public class FtpServer(
    IFileSystemProvider fileSystemProvider,
    IAuthenticationProvider authenticationProvider,
    IOptions<FtpServerConfigurationOptions> configurationOptions,
    ILogger<FtpServer>? logger = null
)
{
    private readonly FtpServerConfigurationOptions _configurationOptions = configurationOptions.Value;

    private readonly IFileSystemProvider _fileSystemProvider =
        fileSystemProvider ?? throw new ArgumentNullException(nameof(fileSystemProvider));

    private readonly IAuthenticationProvider _authenticationProvider =
        authenticationProvider ?? throw new ArgumentNullException(nameof(authenticationProvider));

    private readonly ILogger<FtpServer> _logger = logger ?? NullLogger<FtpServer>.Instance;
    private readonly FtpCommandHandler _commandHandler = new();
    private readonly Dictionary<int, IFtpSession> _sessions = [];

    private NetServer? _netServer;

    private void RegisterCommands()
    {
        _commandHandler.RegisterCommand(new UserCommand());
        _commandHandler.RegisterCommand(new PassCommand());
        _commandHandler.RegisterCommand(new PwdCommand());
        _commandHandler.RegisterCommand(new PasvCommand());
        _commandHandler.RegisterCommand(new ListCommand());
        _commandHandler.RegisterCommand(new QuitCommand());
        _commandHandler.RegisterCommand(new SystCommand());
        _commandHandler.RegisterCommand(new FeatCommand());
        _commandHandler.RegisterCommand(new TypeCommand());
        _commandHandler.RegisterCommand(new OptsCommand());
        _commandHandler.RegisterCommand(new MkdCommand());
        _commandHandler.RegisterCommand(new CwdCommand());
        _commandHandler.RegisterCommand(new StorCommand());
        _commandHandler.RegisterCommand(new RetrCommand());
        _commandHandler.RegisterCommand(new AuthCommand());
        _commandHandler.RegisterCommand(new NoopCommand());
        _commandHandler.RegisterCommand(new SizeCommand());
        _commandHandler.RegisterCommand(new DeleCommand());
        _commandHandler.RegisterCommand(new RmdCommand());
        _commandHandler.RegisterCommand(new RenameCommands());
    }

    /// <summary>
    /// Starts the FTP server and initializes its operations asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of starting the server.</returns>
    public async Task StartAsync()
    {
        if (_netServer is not null)
        {
            throw new InvalidOperationException("The FTP server is already running.");
        }

        RegisterCommands();

        _netServer = new NetServer();
        _netServer.OnClientConnected += async c =>
        {
            _logger.LogInformation("Client connected: {Ip} (Id: {Id})", c.Ip, c.Id);
            var session = new FtpSession(c);
            _sessions[c.Id] = session;

            await session.SendResponseAsync(220, $"{_configurationOptions.ServerName} ready for new user.");
        };

        _netServer.OnClientDataReceived += async (c, data) =>
        {
            if (!_sessions.TryGetValue(c.Id, out var session))
            {
                return;
            }

            var ftpSession = (FtpSession)session;

            using (await ftpSession.LockSessionAsync())
            {
                var rawData = Encoding.UTF8.GetString(data.ToArray());
                ftpSession.CommandBuffer.Append(rawData);

                var bufferContent = ftpSession.CommandBuffer.ToString();
                while (bufferContent.Contains("\r\n"))
                {
                    var index = bufferContent.IndexOf("\r\n", StringComparison.InvariantCulture);
                    var rawRequest = bufferContent[..index];
                    ftpSession.CommandBuffer.Remove(0, index + 2);
                    bufferContent = ftpSession.CommandBuffer.ToString();

                    if (string.IsNullOrWhiteSpace(rawRequest)) continue;

                    _logger.LogInformation("Received [{Id}]: {RawRequest}", c.Id, rawRequest);

                    var parts = rawRequest.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    var verb = parts[0].ToUpperInvariant();
                    var args = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                    var context = new FtpCommandContext(
                        session,
                        verb,
                        args,
                        _fileSystemProvider,
                        _authenticationProvider,
                        _configurationOptions,
                        _logger);
                    await _commandHandler.HandleCommandAsync(context);

                    if (verb != "QUIT") continue;

                    _logger.LogInformation("Client disconnected explicitly: {Id}", c.Id);
                    _sessions.Remove(c.Id);
                    c.Stop();
                    break;
                }
            }
        };

        _netServer.OnClientDisconnected += (c, reason) =>
        {
            _logger.LogInformation("Client disconnected: {Id}. Reason: {Reason}", c.Id, reason);
            _sessions.Remove(c.Id);
        };

        _netServer.OnError += e => _logger.LogError(e, "FTP Server Error");
        _netServer.Start(_configurationOptions.ListeningIp, _configurationOptions.FtpPort);
    }

    /// <summary>
    /// Stops the FTP server and terminates its operations asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of stopping the server.</returns>
    public Task StopAsync()
    {
        if (_netServer is null)
        {
            return Task.CompletedTask;
        }

        _netServer.Stop();
        _netServer = null;
        _logger.LogInformation("FTP Server stopped.");

        return Task.CompletedTask;
    }
}
