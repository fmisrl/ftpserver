using System.Globalization;
using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Commands;

public class SizeCommand : IFtpCommand
{
    public string[] Verbs => ["SIZE"];

    public async Task ExecuteAsync(FtpCommandContext context)
    {
        if (!context.Session.IsAuthenticated)
        {
            await context.Session.SendResponseAsync(530, "Not logged in.");
            return;
        }

        string targetFile = context.Arguments;
        if (!targetFile.StartsWith('/'))
        {
            targetFile = context.Session.CurrentDirectory.TrimEnd('/') + '/' + targetFile;
        }

        try
        {
            var entries = await context.FileSystem.GetEntriesAsync(Path.GetDirectoryName(targetFile) ?? "/");
            var file = entries.FirstOrDefault(e => !e.IsDirectory && e.Name == Path.GetFileName(targetFile));

            if (file != null)
            {
                await context.Session.SendResponseAsync(213, file.Size.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await context.Session.SendResponseAsync(550, "File not found.");
            }
        }
        catch
        {
            await context.Session.SendResponseAsync(550, "Error getting file size.");
        }
    }
}
