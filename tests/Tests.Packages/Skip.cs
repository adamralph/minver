using System.Reflection;
using Fixtures;
using Testing;
using Xunit;

namespace Tests.Packages;

public static class Skip
{
    [Fact]
    public static async Task HasDefaultSdkVersion()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);
        var envVars = ("MinVerSkip".ToAltCase(), "true");
        var expected = Package.WithVersion(1, 0, 0);

        // act
        var (actual, _, _) = await DotNetCli.BuildProject(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
    }
}
