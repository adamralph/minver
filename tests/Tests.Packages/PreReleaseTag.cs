using System.Reflection;
using Fixtures;
using Testing;
using Xunit;

namespace Tests.Packages;

public static class PreReleaseTag
{
    [Fact]
    public static async Task HasTagVersion()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4-alpha.5");

        var expected = Package.WithVersion(2, 3, 4, ["alpha", "5",]);

        // act
        var (actual, _, _) = await DotNetCli.BuildProject(path);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }
}
