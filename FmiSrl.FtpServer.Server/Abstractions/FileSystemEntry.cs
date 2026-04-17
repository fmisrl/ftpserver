namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Represents an entry in the file system.
/// </summary>
/// <param name="Name">The name of the entry.</param>
/// <param name="Size">The size of the entry in bytes.</param>
/// <param name="LastModified">The last modification date and time of the entry.</param>
/// <param name="IsDirectory">A value indicating whether the entry is a directory.</param>
public record FileSystemEntry(
    string Name,
    long Size,
    DateTime LastModified,
    bool IsDirectory
);
