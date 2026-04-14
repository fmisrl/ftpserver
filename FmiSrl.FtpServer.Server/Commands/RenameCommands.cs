using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class RenameCommands : IFtpCommand
{
    public string[] Verbs => ["RNFR", "RNTO"];
    
    private string? _rnfrPath;

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (!context.Session.IsAuthenticated)
        {
            await context.Session.SendResponseAsync(530, "Not logged in.");
            return;
        }

        if (context.Verb == "RNFR")
        {
            if (string.IsNullOrWhiteSpace(context.Arguments))
            {
                await context.Session.SendResponseAsync(501, "Syntax error.");
                return;
            }

            _rnfrPath = context.Arguments;
            if (!_rnfrPath.StartsWith('/'))
            {
                _rnfrPath = context.Session.CurrentDirectory.TrimEnd('/') + '/' + _rnfrPath;
            }

            await context.Session.SendResponseAsync(350, "Requested file action pending further information.");
        }
        else // RNTO
        {
            if (_rnfrPath == null)
            {
                await context.Session.SendResponseAsync(503, "Bad sequence of commands.");
                return;
            }

            string rntoPath = context.Arguments;
            if (!rntoPath.StartsWith('/'))
            {
                rntoPath = context.Session.CurrentDirectory.TrimEnd('/') + '/' + rntoPath;
            }

            try
            {
                await context.FileSystem.RenameAsync(context.AuthContext, _rnfrPath, rntoPath);
                await context.Session.SendResponseAsync(250, "File renamed successfully.");
            }
            catch (Exception ex)
            {
                await context.Session.SendResponseAsync(550, $"Error renaming file: {ex.Message}");
            }
            finally
            {
                _rnfrPath = null;
            }
        }
    }
}
