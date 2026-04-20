using FmiSrl.FtpServer.Server.Infrastructure;

namespace FmiSrl.FtpServer.Tests.Unit.Infrastructure;

public class PathHelperTests
{
    [Theory]
    [InlineData("/", "test", "/test")]
    [InlineData("/dir1", "test", "/dir1/test")]
    [InlineData("/dir1/", "test", "/dir1/test")]
    [InlineData("/", "/absolute/path", "/absolute/path")]
    [InlineData("/dir1", "../dir2", "/dir2")]
    [InlineData("/dir1/dir2", "..", "/dir1")]
    [InlineData("/dir1/dir2", "../../dir3", "/dir3")]
    [InlineData("/dir1", "./test", "/dir1/test")]
    [InlineData("/", "..", "/")]
    [InlineData("/dir1", "subdir/../other", "/dir1/other")]
    public void When_normalizing_path_should_resolve_relative_components(
        string currentDir,
        string path,
        string expected
    )
    {
        // Act
        var result = PathHelper.NormalizePath(currentDir, path);

        // Assert
        Assert.Equal(expected, result);
    }
}
