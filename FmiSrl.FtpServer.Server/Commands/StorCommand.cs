using FmiSrl.FtpServer.Server.Abstractions;
using Microsoft.Extensions.Logging;

namespace FmiSrl.FtpServer.Server.Commands;

public class StorCommand : IFtpCommand
{
    public string[] Verbs => ["STOR"];

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

        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error in parameters or arguments.");
            return;
        }

        string targetFile = context.Arguments;
        
        // Handle relative vs absolute paths
        if (!targetFile.StartsWith('/'))
        {
            targetFile = context.Session.CurrentDirectory.TrimEnd('/') + '/' + targetFile;
        }

        await context.Session.SendResponseAsync(150, "Ok to send data.");

        try
        {
            var dataStream = await context.Session.DataConnection.GetStreamAsync();
            using (var fileStream = await context.FileSystem.OpenWriteAsync(context.AuthContext, targetFile))
            {
                context.Logger.LogInformation("Starting upload of {TargetFile}...", targetFile);
                await dataStream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
                context.Logger.LogInformation("Finished upload of {TargetFile}.", targetFile);
            }
            
            if (dataStream is IAsyncDisposable ad) await ad.DisposeAsync();
            else dataStream.Dispose();

            await context.Session.SendResponseAsync(226, "Transfer complete.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Failed to transfer file: {TargetFile}", targetFile);
            await context.Session.SendResponseAsync(550, $"Failed to transfer file: {ex.Message}");
        }
        finally
        {
            await context.Session.DataConnection.DisposeAsync();
            context.Session.DataConnection = null;
        }
    }
}
