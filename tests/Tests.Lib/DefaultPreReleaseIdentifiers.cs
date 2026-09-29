using System.Reflection;
using MinVer.Lib;
using Testing;
using Tests.Lib.Infra;
using Xunit;
using static Testing.Git;

namespace Tests.Lib;

public static class DefaultPreReleaseIdentifiers
{
    [Theory]
    [InlineData("alpha.0", "0.0.0-alpha.0")]
    [InlineData("preview.x", "0.0.0-preview.x")]
    public static async Task Various(string identifiers, string expectedVersion)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory(identifiers);
        await EnsureEmptyRepositoryAndCommit(path);
        var identifierList = identifiers.Split('.');

        // act
        var actualVersion = await Versioner.GetVersion(path, "", MajorMinor.Default, "", default, identifierList, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }
}
