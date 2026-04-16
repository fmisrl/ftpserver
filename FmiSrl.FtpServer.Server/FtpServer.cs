using System.Diagnostics.CodeAnalysis;
using System.Text;
using Miku.Core;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;
using FmiSrl.FtpServer.Server.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FmiSrl.FtpServer.Server;

/// <summary>
/// Represents an FTP server that handles file transfer operations
/// using the File Transfer Protocol (FTP).
/// </summary>
/// <remarks>
/// This server implementation uses a delegated control architecture where command execution
/// is offloaded to specific <see cref="IFtpCommand"/> implementations.
/// It leverages <see cref="NetServer"/> for asynchronous network I/O.
/// </remarks>
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
        IFtpCommand[] commands = 
        [
            new UserCommand(),
            new PassCommand(),
            new PwdCommand(),
            new PasvCommand(),
            new ListCommand(),
            new QuitCommand(),
            new SystCommand(),
            new FeatCommand(),
            new TypeCommand(),
            new OptsCommand(),
            new MkdCommand(),
            new CwdCommand(),
            new StorCommand(),
            new RetrCommand(),
            new AuthCommand(),
            new NoopCommand(),
            new SizeCommand(),
            new DeleCommand(),
            new RmdCommand(),
            new RenameCommands()
        ];

        foreach (var command in commands)
        {
            _commandHandler.RegisterCommand(command);
        }
    }

    /// <summary>
    /// Starts the FTP server and initializes its operations asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of starting the server.</returns>
    /// <remarks>
    /// Initializes command registration, sets up event handlers for client connections, 
    /// data reception, and disconnections, and then begins listening on the configured IP and port.
    /// </remarks>
    public async Task StartAsync()
    {
        if (_netServer is not null)
        {
            throw new InvalidOperationException("The FTP server is already running.");
        }

        RegisterCommands();

        _netServer = new NetServer();
        _netServer.OnClientConnected += client => _ = HandleClientConnectedAsync(client);
        _netServer.OnClientDataReceived += (client, data) => _ = HandleClientDataReceivedAsync(client, data);
        _netServer.OnClientDisconnected += (client, reason) => HandleClientDisconnected(client, reason);
        _netServer.OnError += exception => HandleError(exception);
        
        _netServer.Start(_configurationOptions.ListeningIp, _configurationOptions.FtpPort);
        await Task.CompletedTask;
    }

    private async Task HandleClientConnectedAsync(NetClient client)
    {
        _logger.LogInformation("Client connected: {Ip} (Id: {Id})", client.Ip, client.Id);
        var session = new FtpSession(client);
        _sessions[client.Id] = session;

        await session.SendResponseAsync(220, $"{_configurationOptions.ServerName} ready for new user.");
    }

    private async Task HandleClientDataReceivedAsync(NetClient client, ReadOnlyMemory<byte> data)
    {
        if (!_sessions.TryGetValue(client.Id, out var session))
        {
            return;
        }

        var ftpSession = (FtpSession)session;
        using (await ftpSession.LockSessionAsync())
        {
            await ProcessClientDataAsync(client, ftpSession, data);
        }
    }

    private async Task ProcessClientDataAsync(NetClient client, FtpSession session, ReadOnlyMemory<byte> data)
    {
        var rawData = Encoding.UTF8.GetString(data.ToArray());
        session.CommandBuffer.Append(rawData);

        // DoS protection: prevent unbounded memory allocation
        if (session.CommandBuffer.Length > 4096)
        {
            _logger.LogWarning("Command buffer exceeded maximum length for client {Id}. Disconnecting.", client.Id);
            await session.SendResponseAsync(500, "Line too long.");
            HandleExplicitQuit(client);
            return;
        }

        while (TryExtractLine(session.CommandBuffer, out var line))
        {
            await ExecuteCommandAsync(client, session, line);
        }
    }

    private static bool TryExtractLine(StringBuilder buffer, [NotNullWhen(true)] out string? line)
    {
        var content = buffer.ToString();
        var index = content.IndexOf("\r\n", StringComparison.InvariantCulture);
        if (index == -1)
        {
            line = null;
            return false;
        }

        line = content[..index];
        buffer.Remove(0, index + 2);
        return true;
    }

    private async Task ExecuteCommandAsync(NetClient client, FtpSession session, string rawRequest)
    {
        if (string.IsNullOrWhiteSpace(rawRequest)) return;

        _logger.LogInformation("Received [{Id}]: {RawRequest}", client.Id, rawRequest);

        var (verb, args) = ParseRequest(rawRequest);

        var context = new FtpCommandContext(
            session,
            verb,
            args,
            _fileSystemProvider,
            _authenticationProvider,
            _configurationOptions,
            _logger);
            
        await _commandHandler.HandleCommandAsync(context);

        if (verb == "QUIT")
        {
            HandleExplicitQuit(client);
        }
    }

    private static (string Verb, string Args) ParseRequest(string rawRequest)
    {
        var parts = rawRequest.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var verb = parts[0].ToUpperInvariant();
        var args = parts.Length > 1 ? parts[1].Trim() : string.Empty;
        return (verb, args);
    }

    private void HandleExplicitQuit(NetClient client)
    {
        _logger.LogInformation("Client disconnected explicitly: {Id}", client.Id);
        _sessions.Remove(client.Id);
        client.Stop();
    }

    private void HandleClientDisconnected(NetClient client, string reason)
    {
        _logger.LogInformation("Client disconnected: {Id}. Reason: {Reason}", client.Id, reason);
        _sessions.Remove(client.Id);
    }

    private void HandleError(Exception exception)
    {
        _logger.LogError(exception, "FTP Server Error");
    }

    /// <summary>
    /// Stops the FTP server and terminates its operations asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of stopping the server.</returns>
    /// <remarks>
    /// Stops the underlying <see cref="NetServer"/> and clears the server state.
    /// </remarks>
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
