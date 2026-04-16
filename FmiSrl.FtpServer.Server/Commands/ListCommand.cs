using System.Text;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the LIST command to retrieve directory information.
/// </summary>
public class ListCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["LIST"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => true;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {        if (context.Session.DataConnection == null)
        {
            await context.Session.SendResponseAsync(425, "Use PASV or PORT first.");
            return;
        }

        await SendDirectoryListingAsync(context);
    }

    private static async Task SendDirectoryListingAsync(FtpCommandContext context)
    {
        await context.Session.SendResponseAsync(150, "Here comes the directory listing.");

        try
        {
            await ProcessDirectoryListingAsync(context);
        }
        finally
        {
            await context.Session.DataConnection!.DisposeAsync();
            context.Session.DataConnection = null;
        }

        await context.Session.SendResponseAsync(226, "Directory send OK.");
    }

    private static async Task ProcessDirectoryListingAsync(FtpCommandContext context)
    {
        var dataStream = await context.Session.DataConnection!.GetStreamAsync();

        await WriteEntriesAsync(context, dataStream, context.AuthContext.Username ?? "anonymous");

        if (dataStream is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            dataStream.Dispose();
        }
    }

    private static async Task WriteEntriesAsync(FtpCommandContext context, Stream dataStream, string username)
    {
        // For LIST without arguments, it defaults to the current directory.
        // If arguments are provided, use them as the target path.
        var targetPath = string.IsNullOrWhiteSpace(context.Arguments)
            ? context.Session.CurrentDirectory
            : PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        await using var writer = new StreamWriter(dataStream, Encoding.UTF8, leaveOpen: true);
        var entries = await context.FileSystem.GetEntriesAsync(context.AuthContext, targetPath);

        foreach (var entry in entries)
        {
            await WriteEntryAsync(writer, entry, username);
        }

        await writer.FlushAsync();
    }

    private static async Task WriteEntryAsync(StreamWriter writer, FileSystemEntry entry, string username)
    {
        var type = entry.IsDirectory ? "d" : "-";
        var line = $"{type}rw-r--r-- 1 {username} {username} {entry.Size} {entry.LastModified:MMM dd HH:mm} {entry.Name}\r\n";
        await writer.WriteAsync(line);
    }
}
