using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the CWD (Change Working Directory) command.
/// </summary>
public class CwdCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["CWD"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => true;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error in parameters or arguments.");
            return;
        }

        await ChangeDirectoryAsync(context);
    }

    private static async Task ChangeDirectoryAsync(FtpCommandContext context)
    {
        var targetDirectory = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        try
        {
            await TryUpdateSessionDirectoryAsync(context, targetDirectory);
        }
        catch (Exception ex)
        {
            await context.Session.SendResponseAsync(550, $"Failed to change directory: {ex.Message}");
        }
    }

    private static async Task TryUpdateSessionDirectoryAsync(FtpCommandContext context, string targetDirectory)
    {
        if (targetDirectory == "/" || await context.FileSystem.DirectoryExistsAsync(context.AuthContext, targetDirectory))
        {
            context.Session.CurrentDirectory = targetDirectory;
            await context.Session.SendResponseAsync(250, "Directory successfully changed.");
            return;
        }

        await context.Session.SendResponseAsync(550, "Failed to change directory. Directory not found.");
    }
}
