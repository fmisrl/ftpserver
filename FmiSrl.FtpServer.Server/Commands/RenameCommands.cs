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

    private string? _rnfrPath;

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

    private async Task HandleRenameFromAsync(FtpCommandContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error.");
            return;
        }

        _rnfrPath = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        await context.Session.SendResponseAsync(350, "Requested file action pending further information.");
    }

    private async Task HandleRenameToAsync(FtpCommandContext context)
    {
        if (_rnfrPath == null)
        {
            await context.Session.SendResponseAsync(503, "Bad sequence of commands.");
            return;
        }

        var rntoPath = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        try
        {
            await TryPerformRenameAsync(context, rntoPath);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Error renaming file.");
            await context.Session.SendResponseAsync(550, "Action failed.");
        }
        finally
        {
            _rnfrPath = null;
        }
    }

    private async Task TryPerformRenameAsync(FtpCommandContext context, string rntoPath)
    {
        await context.FileSystem.RenameAsync(context.AuthContext, _rnfrPath!, rntoPath);
        await context.Session.SendResponseAsync(250, "File renamed successfully.");
    }
}
