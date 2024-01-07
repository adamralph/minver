using System.Reflection;
using System.Threading.Tasks;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;

namespace MinVerTests.Lib;

public static class TagPrefixes
{
    [Theory]
    [InlineData("2.3.4.5", "", "2.3.4.5")]
    [InlineData("v3.4.5.8", "v", "3.4.5.8")]
    [InlineData("version5.6.7.15", "version", "5.6.7.15")]
    [InlineData("2.3.4", "", "2.3.4.0")]
    [InlineData("r3.4.5", "r", "3.4.5.0")]
    [InlineData("revision5.6.7", "revision", "5.6.7.0")]
    public static async Task TagPrefix(string tag, string prefix, string expectedVersion)
    {
        // act
        var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, prefix));
        await EnsureEmptyRepositoryAndCommit(path);
        await Tag(path, tag);

        // act
        var actualVersion = Versioner.GetVersion(path, prefix, MajorMinor.Default, "", default, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }
}
