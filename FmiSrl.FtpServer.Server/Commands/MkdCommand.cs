using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class MkdCommand : IFtpCommand
{
    public string[] Verbs => ["MKD"];

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

        string targetDirectory = context.Arguments;
        
        // Handle relative vs absolute paths simply
        if (!targetDirectory.StartsWith('/'))
        {
            targetDirectory = context.Session.CurrentDirectory.TrimEnd('/') + '/' + targetDirectory;
        }

        try
        {
            if (await context.FileSystem.DirectoryExistsAsync(targetDirectory))
            {
                await context.Session.SendResponseAsync(550, "Directory already exists.");
                return;
            }

            await context.FileSystem.CreateDirectoryAsync(targetDirectory);
            await context.Session.SendResponseAsync(257, $"\"{targetDirectory}\" directory created.");
        }
        catch (Exception ex)
        {
            await context.Session.SendResponseAsync(550, $"Failed to create directory: {ex.Message}");
        }
    }
}
