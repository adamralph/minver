using System.Reflection;
using Fixtures;
using Tests.Packages.Fixtures;
using Xunit;

namespace Tests.Packages;

public static class VersionOverride
{
    [Fact]
    public static async Task HasVersionOverride()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);

        await Git.InitAsync(path);
        await Git.CommitAsync(path);
        await Git.TagAsync(path, "2.3.4");

        var envVars = ("MinVerVersionOverride".ToAltCase(), "3.4.5-alpha.6+build.7");

        var expected = Package.WithVersion(3, 4, 5, ["alpha", "6",], 0, "build.7");

        // act
        var (actual, _, _) = await DotNetCli.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.InformationalVersion, cliStandardOutput.Trim());
    }
}
