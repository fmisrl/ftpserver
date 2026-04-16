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
    {        await ProcessSizeRequestAsync(context);
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
        var directory = Path.GetDirectoryName(targetFile)?.Replace('\\', '/') ?? "/";
        if (directory == string.Empty)
        {
            directory = "/";
        }

        var entries = await context.FileSystem.GetEntriesAsync(context.AuthContext, directory);
        var file = entries.FirstOrDefault(e => !e.IsDirectory && e.Name == Path.GetFileName(targetFile));

        if (file != null)
        {
            await context.Session.SendResponseAsync(213, file.Size.ToString(CultureInfo.InvariantCulture));
            return;
        }

        await context.Session.SendResponseAsync(550, "File not found.");
    }
}
