using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class RmdCommand : IFtpCommand
{
    public string[] Verbs => ["RMD"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (!context.Session.IsAuthenticated)
        {
            await context.Session.SendResponseAsync(530, "Not logged in.");
            return;
        }

        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error in parameters or arguments.");
            return;
        }

        string targetDir = context.Arguments;
        if (!targetDir.StartsWith('/'))
        {
            targetDir = context.Session.CurrentDirectory.TrimEnd('/') + '/' + targetDir;
        }

        try
        {
            if (await context.FileSystem.DirectoryExistsAsync(targetDir))
            {
                await context.FileSystem.DeleteDirectoryAsync(targetDir);
                await context.Session.SendResponseAsync(250, "Directory deleted successfully.");
            }
            else
            {
                await context.Session.SendResponseAsync(550, "Directory not found.");
            }
        }
        catch (Exception ex)
        {
            await context.Session.SendResponseAsync(550, $"Error deleting directory: {ex.Message}");
        }
    }
}
