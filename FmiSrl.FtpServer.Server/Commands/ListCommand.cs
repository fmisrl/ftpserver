using System.Text;
using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class ListCommand : IFtpCommand
{
    public string[] Verbs => ["LIST"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (!context.Session.IsAuthenticated)
        {
            await context.Session.SendResponseAsync(530, "Not logged in.");
            return;
        }

        if (context.Session.DataConnection == null)
        {
            await context.Session.SendResponseAsync(425, "Use PASV or PORT first.");
            return;
        }

        await context.Session.SendResponseAsync(150, "Here comes the directory listing.");

        try
        {
            var dataStream = await context.Session.DataConnection.GetStreamAsync();
            using (var writer = new StreamWriter(dataStream, Encoding.UTF8, leaveOpen: true))
            {
                var entries = await context.FileSystem.GetEntriesAsync(context.Session.CurrentDirectory);
                foreach (var entry in entries)
                {
                    // Simple Unix-style listing format
                    string type = entry.IsDirectory ? "d" : "-";
                    string line = $"{type}rw-r--r-- 1 ftp ftp {entry.Size} {entry.LastModified:MMM dd HH:mm} {entry.Name}\r\n";
                    await writer.WriteAsync(line);
                }
                await writer.FlushAsync();
            }

            if (dataStream is IAsyncDisposable ad) await ad.DisposeAsync();
            else dataStream.Dispose();
        }
        finally
        {
            await context.Session.DataConnection.DisposeAsync();
            context.Session.DataConnection = null;
        }

        await context.Session.SendResponseAsync(226, "Directory send OK.");
    }
}
