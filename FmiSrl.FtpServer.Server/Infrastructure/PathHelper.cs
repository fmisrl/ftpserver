namespace FmiSrl.FtpServer.Server.Infrastructure;

/// <summary>
/// Provides utility methods for handling FTP paths.
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// Normalizes an FTP path by resolving relative components and ensuring a consistent format.
    /// </summary>
    /// <param name="currentDirectory">The current working directory.</param>
    /// <param name="path">The path to normalize.</param>
    /// <returns>A normalized absolute FTP path.</returns>
    public static string NormalizePath(string currentDirectory, string path)
    {
        string absolutePath = path.StartsWith('/') 
            ? path 
            : $"{currentDirectory.TrimEnd('/')}/{path}";

        var parts = absolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var resolved = new List<string>();

        foreach (var part in parts)
        {
            ProcessPathPart(resolved, part);
        }
        
        return "/" + string.Join('/', resolved);
    }

    private static void ProcessPathPart(List<string> resolved, string part)
    {
        if (part == ".") return;
        
        if (part == "..")
        {
            HandleParentDirectory(resolved);
            return;
        }

        resolved.Add(part);
    }

    private static void HandleParentDirectory(List<string> resolved)
    {
        if (resolved.Count > 0)
        {
            resolved.RemoveAt(resolved.Count - 1);
        }
    }
}
