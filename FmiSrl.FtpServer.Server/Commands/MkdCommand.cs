using FmiSrl.FtpServer.Server.Abstractions;
using FmiSrl.FtpServer.Server.Infrastructure;

namespace FmiSrl.FtpServer.Server.Commands;

/// <summary>
/// Implements the MKD (Make Directory) command.
/// </summary>
public class MkdCommand : IFtpCommand
{
    /// <inheritdoc/>
    public string[] Verbs => ["MKD"];

    /// <inheritdoc/>
    public bool RequiresAuthentication => true;

    /// <inheritdoc/>
    public async Task ExecuteAsync(FtpCommandContext context)
    {        if (string.IsNullOrWhiteSpace(context.Arguments))
        {
            await context.Session.SendResponseAsync(501, "Syntax error in parameters or arguments.");
            return;
        }

        await CreateDirectoryAsync(context);
    }

    private static async Task CreateDirectoryAsync(FtpCommandContext context)
    {
        var targetDirectory = PathHelper.NormalizePath(context.Session.CurrentDirectory, context.Arguments);

        try
        {
            await TryPerformCreateAsync(context, targetDirectory);
        }
        catch (Exception ex)
        {
            await context.Session.SendResponseAsync(550, $"Failed to create directory: {ex.Message}");
        }
    }

    private static async Task TryPerformCreateAsync(FtpCommandContext context, string targetDirectory)
    {
        if (await context.FileSystem.DirectoryExistsAsync(context.AuthContext, targetDirectory))
        {
            await context.Session.SendResponseAsync(550, "Directory already exists.");
            return;
        }

        await context.FileSystem.CreateDirectoryAsync(context.AuthContext, targetDirectory);
        await context.Session.SendResponseAsync(257, $"\"{targetDirectory}\" directory created.");
    }
}
