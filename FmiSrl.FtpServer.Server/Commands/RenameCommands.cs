using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the RNFR (Rename From) and RNTO (Rename To) commands for renaming files.
/// </summary>
public class RenameCommands : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["RNFR", "RNTO"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => true;

    private const string RnfrPathKey = "RnfrPath";

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (context.Verb == "RNFR")
        {
            await HandleRenameFromAsync(context);
        }
        else
        {
            await HandleRenameToAsync(context);
        }
    }

    private static async Task HandleRenameFromAsync(FtpCommandContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error.");
            return;
        }

        var rnfrPath = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);
        context.Session.State[RnfrPathKey] = rnfrPath;

        await context.Session.SendResponseAsync(350, "Requested file action pending further information.");
    }

    private static async Task HandleRenameToAsync(FtpCommandContext context)
    {
        if (!context.Session.State.TryGetValue(RnfrPathKey, out var rnfrPathObj) || rnfrPathObj is not string rnfrPath)
        {
            await context.Session.SendResponseAsync(503, "Bad sequence of commands.");
            return;
        }

        var rntoPath = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        try
        {
            await TryPerformRenameAsync(context, rnfrPath, rntoPath);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Error renaming file.");
            await context.Session.SendResponseAsync(550, "Action failed.");
        }
        finally
        {
            context.Session.State.Remove(RnfrPathKey);
        }
    }

    private static async Task TryPerformRenameAsync(FtpCommandContext context, string rnfrPath, string rntoPath)
    {
        await context.FileSystem.RenameAsync(context.AuthContext, rnfrPath, rntoPath);
        await context.Session.SendResponseAsync(250, "File renamed successfully.");
    }
}
