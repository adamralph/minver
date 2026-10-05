using System.Reflection;
using Fixtures;
using MinVer.Lib;
using Tests.Lib.Fixtures;
using Xunit;
using static Fixtures.Git;

namespace Tests.Lib;

public static class BuildMetadata
{
    [Theory]
    [InlineData("", "0.0.0-alpha.0")]
    [InlineData("a", "0.0.0-alpha.0+a")]
    public static async Task NoCommits(string buildMetadata, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory(buildMetadata);
        await EnsureEmptyRepositoryAsync(path);

        // act
        var actualVersion = await Versioner.GetVersionAsync(path, "", MajorMinor.Default, buildMetadata, default, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }

    [Theory]
    [InlineData("", "0.0.0-alpha.0")]
    [InlineData("a", "0.0.0-alpha.0+a")]
    public static async Task NoTag(string buildMetadata, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory(buildMetadata);
        await EnsureEmptyRepositoryAndCommitAsync(path);

        // act
        var actualVersion = await Versioner.GetVersionAsync(path, "", MajorMinor.Default, buildMetadata, default, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }

    [Theory]
    [InlineData("1.2.3+a", "", "1.2.3+a")]
    [InlineData("1.2.3", "b", "1.2.3+b")]
    [InlineData("1.2.3+a", "b", "1.2.3+a.b")]
    [InlineData("1.2.3-pre+a", "", "1.2.3-pre+a")]
    [InlineData("1.2.3-pre", "b", "1.2.3-pre+b")]
    [InlineData("1.2.3-pre+a", "b", "1.2.3-pre+a.b")]
    public static async Task CurrentTag(string tag, string buildMetadata, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, buildMetadata));
        await EnsureEmptyRepositoryAndCommitAsync(path);
        await TagAsync(path, tag);

        // act
        var actualVersion = await Versioner.GetVersionAsync(path, "", MajorMinor.Default, buildMetadata, default, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }

    [Theory]
    [InlineData("1.2.3+a", "", "1.2.4-alpha.0.1")]
    [InlineData("1.2.3", "b", "1.2.4-alpha.0.1+b")]
    [InlineData("1.2.3+a", "b", "1.2.4-alpha.0.1+b")]
    [InlineData("1.2.3-pre+a", "", "1.2.3-pre.1")]
    [InlineData("1.2.3-pre", "b", "1.2.3-pre.1+b")]
    [InlineData("1.2.3-pre+a", "b", "1.2.3-pre.1+b")]
    public static async Task PreviousTag(string tag, string buildMetadata, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, buildMetadata));
        await EnsureEmptyRepositoryAndCommitAsync(path);
        await TagAsync(path, tag);
        await CommitAsync(path);

        // act
        var actualVersion = await Versioner.GetVersionAsync(path, "", MajorMinor.Default, buildMetadata, default, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }
}
