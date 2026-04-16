using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the RMD (Remove Directory) command.
/// </summary>
public class RmdCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["RMD"];

    /// <inheritdoc/>
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

        await RemoveDirectoryAsync(context);
    }

    private static async Task RemoveDirectoryAsync(FtpCommandContext context)
    {
        var targetDir = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        try
        {
            await TryPerformRemoveAsync(context, targetDir);
        }
        catch (Exception ex)
        {
            await context.Session.SendResponseAsync(550, $"Error deleting directory: {ex.Message}");
        }
    }

    private static async Task TryPerformRemoveAsync(FtpCommandContext context, string targetDir)
    {
        if (!await context.FileSystem.DirectoryExistsAsync(context.AuthContext, targetDir))
        {
            await context.Session.SendResponseAsync(550, "Directory not found.");
            return;
        }

        await context.FileSystem.DeleteDirectoryAsync(context.AuthContext, targetDir);
        await context.Session.SendResponseAsync(250, "Directory deleted successfully.");
    }
}
