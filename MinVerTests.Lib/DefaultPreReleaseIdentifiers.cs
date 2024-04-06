using System.Reflection;
using System.Threading.Tasks;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;

namespace MinVerTests.Lib;

public static class DefaultPreReleaseIdentifiers
{
    [Theory]
    [InlineData("1.2.3-rc.1", VersionPart.Major, "1.2.3-rc.1.1")]
    [InlineData("1.2.3-rc.1", VersionPart.Minor, "1.2.3-rc.1.1")]
    [InlineData("1.2.3-rc.1", VersionPart.Patch, "1.2.3-rc.1.1")]
    public static async Task PreReleaseVersionIncrement(string tag, VersionPart autoIncrement, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, autoIncrement));
        await EnsureEmptyRepositoryAndCommit(path);
        await Tag(path, tag);
        await Commit(path);

        // act
        var actualVersion = Versioner.GetVersion(path, "", MajorMinor.Default, "", autoIncrement, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }

    [Theory]
    [InlineData("1.2.3", "alpha", "1.2.4-alpha.1")]
    [InlineData("1.2.3-rc.1", "alpha", "1.2.3-rc.1.alpha.1")]
    [InlineData("0.0.0", "preview.x", "0.0.1-preview.x.1")]
    public static async Task AlwaysIncludeDefaultPreRelease(string tag, string identifiers, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory(identifiers);
        await EnsureEmptyRepositoryAndCommit(path);
        await Tag(path, tag);
        await Commit(path);

#pragma warning disable CA1062 // Validate arguments of public methods
        var identifierList = identifiers.Split('.');
#pragma warning restore CA1062 // Validate arguments of public methods

        // act
        var actualData = Versioner.GetVersionData(path, "", MajorMinor.Default, "", default, identifierList, false, true, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualData.Version);
    }

    [Theory]
    [InlineData("alpha.0", "0.0.0-alpha.0")]
    [InlineData("preview.x", "0.0.0-preview.x")]
    public static async Task Various(string identifiers, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory(identifiers);
        await EnsureEmptyRepositoryAndCommit(path);
#pragma warning disable CA1062 // Validate arguments of public methods
        var identifierList = identifiers.Split('.');
#pragma warning restore CA1062 // Validate arguments of public methods

        // act
        var actualVersion = Versioner.GetVersion(path, "", MajorMinor.Default, "", default, identifierList, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }
}
