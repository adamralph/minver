using System.Reflection;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class TagWithPrefix
{
    [Fact]
    public static async Task HasTagVersion()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "v.2.3.4-alpha.5");

        var envVars = ("MinVerTagPrefix", "v.");

        var expected = Package.WithVersion(2, 3, 4, ["alpha", "5",]);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }
}
