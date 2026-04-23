using System.Globalization;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the SIZE command to return the size of a file.
/// </summary>
public class SizeCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["SIZE"];

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
        await ProcessSizeRequestAsync(context);
    }

    private static async Task ProcessSizeRequestAsync(FtpCommandContext context)
    {
        var targetFile = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        try
        {
            await TryGetFileSizeAsync(context, targetFile);
        }
        catch
        {
            await context.Session.SendResponseAsync(550, "Error getting file size.");
        }
    }

    private static async Task TryGetFileSizeAsync(FtpCommandContext context, string targetFile)
    {
        if (await context.FileSystem.FileExistsAsync(context.AuthContext, targetFile))
        {
            var size = await context.FileSystem.GetFileSizeAsync(context.AuthContext, targetFile);
            await context.Session.SendResponseAsync(213, size.ToString(CultureInfo.InvariantCulture));
            return;
        }

        await context.Session.SendResponseAsync(550, "File not found or is a directory.");
    }
}
