using System.Reflection;
using Fixtures;
using Tests.Packages.Fixtures;
using Xunit;

namespace Tests.Packages;

public static class NoRepo
{
    [Fact]
    public static async Task HasDefaultVersion()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);
        var expected = Package.WithVersion(0, 0, 0, ["alpha", "0",]);

        // act
        var (actual, sdkStandardOutput, _) = await DotNetCli.BuildProject(path);
        var (cliStandardOutput, cliStandardError) = await MinVerCli.ReadAsync(path);

        // assert
        Assert.Equal(expected, actual);
        Assert.Contains("MINVER1001", sdkStandardOutput, StringComparison.Ordinal);

        Assert.Equal(expected.Version, cliStandardOutput.Trim());
        Assert.Contains("not a valid Git working directory", cliStandardError, StringComparison.Ordinal);
    }
}
