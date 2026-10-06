using System.Reflection;
using Fixtures;
using Tests.Packages.Fixtures;
using Xunit;

namespace Tests.Packages;

public static class TagWithPrefix
{
    [Fact]
    public static async Task HasTagVersion()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);

        await Git.InitAsync(path);
        await Git.CommitAsync(path);
        await Git.TagAsync(path, "v.2.3.4-alpha.5");

        var envVars = ("MinVerTagPrefix", "v.");

        var expected = Package.WithVersion(2, 3, 4, ["alpha", "5",]);

        // act
        var (actual, _, _) = await DotNetCli.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }
}
