using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class DeleCommand : IFtpCommand
{
    public string[] Verbs => ["DELE"];

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

        string targetFile = context.Arguments;
        if (!targetFile.StartsWith('/'))
        {
            targetFile = context.Session.CurrentDirectory.TrimEnd('/') + '/' + targetFile;
        }

        try
        {
            if (await context.FileSystem.FileExistsAsync(context.AuthContext, targetFile))
            {
                await context.FileSystem.DeleteFileAsync(context.AuthContext, targetFile);
                await context.Session.SendResponseAsync(250, "File deleted successfully.");
            }
            else
            {
                await context.Session.SendResponseAsync(550, "File not found.");
            }
        }
        catch (Exception ex)
        {
            await context.Session.SendResponseAsync(550, $"Error deleting file: {ex.Message}");
        }
    }
}
