using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the STOR (Store) command to upload a file to the server.
/// </summary>
public class StorCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["STOR"];

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

        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error in parameters or arguments.");
            return;
        }

        await ProcessStoreAsync(context);
    }

    private static async Task ProcessStoreAsync(FtpCommandContext context)
    {
        var targetFile = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        await context.Session.SendResponseAsync(150, "Ok to send data.");

        try
        {
            await ReceiveFileAsync(context, targetFile);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Failed to transfer file: {TargetFile}", targetFile);
            await context.Session.SendResponseAsync(550, "Action failed.");
        }
        finally
        {
            await context.Session.DataConnection!.DisposeAsync();
            context.Session.DataConnection = null;
        }
    }

    private static async Task ReceiveFileAsync(FtpCommandContext context, string targetFile)
    {
        var dataStream = await context.Session.DataConnection!.GetStreamAsync();

        using (var fileStream = await context.FileSystem.OpenWriteAsync(context.AuthContext, targetFile))
        {
            context.Logger.LogInformation("Starting upload of {TargetFile}...", targetFile);
            await dataStream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
            context.Logger.LogInformation("Finished upload of {TargetFile}.", targetFile);
        }

        if (dataStream is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            dataStream.Dispose();
        }

        await context.Session.SendResponseAsync(226, "Transfer complete.");
    }
}
