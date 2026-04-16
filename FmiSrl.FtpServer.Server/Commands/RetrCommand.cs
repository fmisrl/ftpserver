using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the RETR (Retrieve) command to download a file from the server.
/// </summary>
public class RetrCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["RETR"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => true;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {        if (context.Session.DataConnection == null)
        {
            await context.Session.SendResponseAsync(425, "Use PASV or PORT first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error in parameters or arguments.");
            return;
        }

        await ProcessRetrieveAsync(context);
    }

    private static async Task ProcessRetrieveAsync(FtpCommandContext context)
    {
        var targetFile = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        if (!await context.FileSystem.FileExistsAsync(context.AuthContext, targetFile))
        {
            await context.Session.SendResponseAsync(550, "File not found.");
            return;
        }

        await context.Session.SendResponseAsync(150, "Ok to send data.");

        try
        {
            await TransferFileAsync(context, targetFile);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Failed to transfer file: {TargetFile}", targetFile);
            await context.Session.SendResponseAsync(550, $"Failed to transfer file: {ex.Message}");
        }
        finally
        {
            await context.Session.DataConnection!.DisposeAsync();
            context.Session.DataConnection = null;
        }
    }

    private static async Task TransferFileAsync(FtpCommandContext context, string targetFile)
    {
        var dataStream = await context.Session.DataConnection!.GetStreamAsync();
        
        using (var fileStream = await context.FileSystem.OpenReadAsync(context.AuthContext, targetFile))
        {
            context.Logger.LogInformation("Starting transfer of {TargetFile} ({Length} bytes)...", targetFile, fileStream.Length);
            await fileStream.CopyToAsync(dataStream);
            context.Logger.LogInformation("Finished copying {TargetFile} to data stream.", targetFile);
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
