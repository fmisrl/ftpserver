namespace FmiSrl.FtpServer.Server.Abstractions;

/// <summary>
/// Defines the behavior for a file system provider used by the FTP server.
/// </summary>
public interface IFileSystemProvider
{
    /// <summary>
    /// Gets the file system entries for the specified path.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="path">The path to get entries for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="FileSystemEntry"/>.</returns>
    Task<IEnumerable<FileSystemEntry>> GetEntriesAsync(FtpAuthenticationContext authContext, string path);

    /// <summary>
    /// Opens a stream for reading a file at the specified path.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Stream"/> for reading the file.</returns>
    Task<Stream> OpenReadAsync(FtpAuthenticationContext authContext, string path);

    /// <summary>
    /// Opens a stream for writing to a file at the specified path.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="path">The path of the file to write.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Stream"/> for writing to the file.</returns>
    Task<Stream> OpenWriteAsync(FtpAuthenticationContext authContext, string path);

    /// <summary>
    /// Deletes a file at the specified path.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="path">The path of the file to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteFileAsync(FtpAuthenticationContext authContext, string path);

    /// <summary>
    /// Creates a directory at the specified path.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="path">The path of the directory to create.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateDirectoryAsync(FtpAuthenticationContext authContext, string path);

    /// <summary>
    /// Deletes a directory at the specified path.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="path">The path of the directory to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteDirectoryAsync(FtpAuthenticationContext authContext, string path);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="path">The path of the file to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    Task<bool> FileExistsAsync(FtpAuthenticationContext authContext, string path);

    /// <summary>
    /// Checks if a directory exists at the specified path.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="path">The path of the directory to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <c>true</c> if the directory exists; otherwise, <c>false</c>.</returns>
    Task<bool> DirectoryExistsAsync(FtpAuthenticationContext authContext, string path);

    /// <summary>
    /// Gets a single file system entry for the specified path.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="path">The path to get the entry for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="FileSystemEntry"/> or <c>null</c> if not found.</returns>
    Task<FileSystemEntry?> GetEntryAsync(FtpAuthenticationContext authContext, string path);

    /// <summary>
    /// Renames a file or directory.
    /// </summary>
    /// <param name="authContext">The authentication context for the operation.</param>
    /// <param name="oldPath">The current path of the file or directory.</param>
    /// <param name="newPath">The new path for the file or directory.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RenameAsync(FtpAuthenticationContext authContext, string oldPath, string newPath);
}
