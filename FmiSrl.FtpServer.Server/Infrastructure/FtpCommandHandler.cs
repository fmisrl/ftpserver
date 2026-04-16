using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Infrastructure;

/// <summary>
/// Handles the registration and execution of FTP commands.
/// </summary>
public class FtpCommandHandler
{
    private readonly Dictionary<string, IFtpCommand> _commands = new(StringComparer.OrdinalIgnoreCase);

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
        if (_commands.TryGetValue(context.Verb, out var command))
        {
            await command.ExecuteAsync(context);
        }
        else
        {
            await context.Session.SendResponseAsync(502, "Command not implemented.");
        }
    }
}
