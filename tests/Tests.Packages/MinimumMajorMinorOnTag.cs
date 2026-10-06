using System.Reflection;
using Fixtures;
using Tests.Packages.Fixtures;
using Xunit;

namespace Tests.Packages;

public static class MinimumMajorMinorOnTag
{
    [Fact]
    public static async Task HasMinimumMajorMinor()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);

        await Git.InitAsync(path);
        await Git.CommitAsync(path);
        await Git.TagAsync(path, "2.3.4");

        var envVars = ("MinVerMinimumMajorMinor".ToAltCase(), "3.0");

        var expected = Package.WithVersion(2, 3, 4);

        // act
        var (actual, _, _) = await DotNetCli.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }
}
