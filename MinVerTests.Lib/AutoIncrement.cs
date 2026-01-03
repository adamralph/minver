using System.Reflection;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;

namespace MinVerTests.Lib;

public static class AutoIncrement
{
    private static Ct Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("1.2.3", VersionPart.Major, "2.0.0-alpha.0.1")]
    [InlineData("1.2.3", VersionPart.Minor, "1.3.0-alpha.0.1")]
    [InlineData("1.2.3", VersionPart.Patch, "1.2.4-alpha.0.1")]
    public static async Task RtmVersionIncrement(string tag, VersionPart autoIncrement, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, autoIncrement));
        await EnsureEmptyRepositoryAndCommit(path, Ct);
        await Tag(path, tag, Ct);
        await Commit(path, Ct);

        // act
        var actualVersion = await Versioner.GetVersion(path, "", MajorMinor.Default, "", autoIncrement, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }
}
