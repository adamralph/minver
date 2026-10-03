using System.Reflection;
using Fixtures;
using Testing;
using Xunit;

namespace Tests.Packages;

public static class CustomAutoIncrement
{
    [Fact]
    public static async Task HasCustomAutoIncrement()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);

        var envVars = ("MinVerAutoIncrement".ToAltCase(), "minor");

        var expected = Package.WithVersion(2, 4, 0, ["alpha", "0",], 1);

        // act
        var (actual, _, _) = await DotNetCli.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }
}
