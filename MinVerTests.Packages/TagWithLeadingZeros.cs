using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class TagWithLeadingZeros
{
    [Fact]
    public static async Task HasTagVersion()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "02.03.04-alpha.05+build.06");

        var envVars = ("MinVerIgnoreLeadingZeros", "true");

        var expected = Package.WithVersion(2, 3, 4, new[] { "alpha", "5", }, buildMetadata: "build.06");

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }
}
