using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Infrastructure;

public class FtpCommandHandler
{
    private readonly Dictionary<string, IFtpCommand> _commands = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterCommand(IFtpCommand command)
    {
        foreach (var verb in command.Verbs)
        {
            _commands[verb] = command;
        }
    }

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
