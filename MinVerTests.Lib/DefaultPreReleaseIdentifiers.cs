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
    [InlineData("alpha.0", "0.0.0.0-alpha.0")]
    [InlineData("preview.x", "0.0.0.0-preview.x")]
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
