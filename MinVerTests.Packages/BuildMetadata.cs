using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class BuildMetadata
{
    [Fact]
    public static async Task HasBuildMetadata()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);
        var envVars = ("MinVerBuildMetadata", "build.123");
        var expected = Package.WithVersion(0, 0, 0, 0, ["alpha", "0",], 0, "build.123");

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.InformationalVersion, cliStandardOutput.Trim());
    }
}
