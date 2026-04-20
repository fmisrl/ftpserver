using System.Text;
using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the NLST command to retrieve a list of file names.
/// </summary>
public class NlstCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["NLST"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => true;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (context.Session.DataConnection == null)
        {
            await context.Session.SendResponseAsync(425, "Use PASV or PORT first.");
            return;
        }

        await context.Session.SendResponseAsync(150, "Here comes the directory listing.");

        try
        {
            var dataStream = await context.Session.DataConnection.GetStreamAsync();
            var targetPath = string.IsNullOrWhiteSpace(context.Arguments)
                ? context.Session.CurrentDirectory
                : PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

            await using var writer = new StreamWriter(dataStream, new UTF8Encoding(false), leaveOpen: true);
            var entries = await context.FileSystem.GetEntriesAsync(context.AuthContext, targetPath);

            foreach (var entry in entries)
            {
                await writer.WriteAsync($"{entry.Name}\r\n");
            }
            await writer.FlushAsync();

            if (dataStream is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                dataStream.Dispose();
            }
        }
        finally
        {
            await context.Session.DataConnection.DisposeAsync();
            context.Session.DataConnection = null;
        }

        await context.Session.SendResponseAsync(226, "Directory send OK.");
    }
}
