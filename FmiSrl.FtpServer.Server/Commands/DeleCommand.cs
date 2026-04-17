using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the DELE command to delete a file.
/// </summary>
public class DeleCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["DELE"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => true;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error in parameters or arguments.");
            return;
        }

        await DeleteFileAsync(context);
    }

    private static async Task DeleteFileAsync(FtpCommandContext context)
    {
        var targetFile = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        try
        {
            await TryPerformDeleteAsync(context, targetFile);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Error deleting file.");
            await context.Session.SendResponseAsync(550, "Action failed.");
        }
    }

    private static async Task TryPerformDeleteAsync(FtpCommandContext context, string targetFile)
    {
        if (!await context.FileSystem.FileExistsAsync(context.AuthContext, targetFile))
        {
            await context.Session.SendResponseAsync(550, "File not found.");
            return;
        }

        await context.FileSystem.DeleteFileAsync(context.AuthContext, targetFile);
        await context.Session.SendResponseAsync(250, "File deleted successfully.");
    }
}
