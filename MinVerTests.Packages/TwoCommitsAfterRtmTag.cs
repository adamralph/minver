using System.Reflection;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class TwoCommitsAfterRtmTag
{
    [Fact]
    public static async Task HasNextPatchWithHeightTwo()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.6");
        await Git.Commit(path);
        await Git.Commit(path);

        var expected = Package.WithVersion(2, 3, 8, ["alpha", "0",], 2);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }
}
