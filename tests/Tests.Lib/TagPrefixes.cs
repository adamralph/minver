using System.Reflection;
using Fixtures;
using MinVer.Lib;
using Tests.Lib.Fixtures;
using Xunit;
using static Fixtures.Git;

namespace Tests.Lib;

public static class TagPrefixes
{
    [Theory]
    [InlineData("2.3.4", "", "2.3.4")]
    [InlineData("v3.4.5", "v", "3.4.5")]
    [InlineData("version5.6.7", "version", "5.6.7")]
    public static async Task TagPrefix(string tag, string prefix, string expectedVersion)
    {
        // act
        var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, prefix));
        await EnsureEmptyRepositoryAndCommitAsync(path);
        await TagAsync(path, tag);

        // act
        var actualVersion = await Versioner.GetVersionAsync(path, prefix, MajorMinor.Default, "", default, PreReleaseIdentifiers.Default, false, NullLogger.Instance);

        // assert
        Assert.Equal(expectedVersion, actualVersion.ToString());
    }
}
