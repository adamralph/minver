using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class IgnoringHeight
{
    [Fact]
    public static async Task HeightIsIgnored()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);

        var envVars = ("MinVerIgnoreHeight".ToAltCase(), "true".ToAltCase());

        var expected = Package.WithVersion(2, 3, 4);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }
}
