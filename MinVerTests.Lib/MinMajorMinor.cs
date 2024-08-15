using System.Reflection;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;

namespace MinVerTests.Lib;

public static class MinMajorMinor
{
    [Fact]
    public static async Task NoCommits()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await EnsureEmptyRepository(path);

        // act
        var actualVersion = Versioner.GetVersion(path, "", new MajorMinor(1, 2), "", default, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal("1.2.0-alpha.0", actualVersion.ToString());
    }

    [Theory]
    [InlineData("4.0.0", 3, 2, "4.0.0")]
    [InlineData("4.3.0", 4, 3, "4.3.0")]
    [InlineData("4.3.0", 5, 4, "4.3.0")]
    public static async Task Tagged(string tag, int major, int minor, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, major, minor));
        await EnsureEmptyRepositoryAndCommit(path);
        await Tag(path, tag);
        var logger = new TestLogger();

        // act
        var actualVersion = Versioner.GetVersion(path, "", new MajorMinor(major, minor), "", default, PreReleaseIdentifiers.Default, false, logger);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());

        Assert.Contains(logger.Messages, message => message.Text.Contains($"Ignoring minimum major minor {major}.{minor} because the commit is tagged.", StringComparison.Ordinal));
    }

    [Fact]
    public static async Task NotTagged()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await EnsureEmptyRepositoryAndCommit(path);

        // act
        var actualVersion = Versioner.GetVersion(path, "", new MajorMinor(1, 0), "", default, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal("1.0.0-alpha.0", actualVersion.ToString());
    }
}
