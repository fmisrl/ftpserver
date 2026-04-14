using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class CwdCommand : IFtpCommand
{
    public string[] Verbs => ["CWD"];

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
        
        // Handle relative vs absolute paths
        if (!targetDirectory.StartsWith('/'))
        {
            targetDirectory = context.Session.CurrentDirectory.TrimEnd('/') + '/' + targetDirectory;
        }

        // Normalize path (handle ".." and ".")
        var parts = targetDirectory.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var resolved = new List<string>();
        foreach (var part in parts)
        {
            if (part == ".") continue;
            if (part == "..") 
            { 
                if (resolved.Count > 0) resolved.RemoveAt(resolved.Count - 1); 
            }
            else 
            {
                resolved.Add(part);
            }
        }
        
        targetDirectory = "/" + string.Join('/', resolved);

        try
        {
            // Root directory always exists virtually
            if (targetDirectory == "/" || await context.FileSystem.DirectoryExistsAsync(context.AuthContext, targetDirectory))
            {
                context.Session.CurrentDirectory = targetDirectory;
                await context.Session.SendResponseAsync(250, "Directory successfully changed.");
            }
            else
            {
                await context.Session.SendResponseAsync(550, "Failed to change directory. Directory not found.");
            }
        }
        catch (Exception ex)
        {
            await context.Session.SendResponseAsync(550, $"Failed to change directory: {ex.Message}");
        }
    }
}
