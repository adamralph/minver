using System.Reflection;
using Fixtures;
using Microsoft.Extensions.FileSystemGlobbing;
using Testing;
using Xunit;

namespace Tests.Packages;

public static class Cleaning
{
    [Theory]
    [InlineData(false)]
    // In the .NET 10 SDK, packages produced from multi-TFM projects are no longer cleaned.
    // This is left here as a placeholder in case this is a regression which will be fixed later.
    // See https://github.com/dotnet/sdk/issues/52109
#if !NET10_0
    [InlineData(true)]
#endif
    public static async Task PackagesAreCleaned(bool multiTarget)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory(multiTarget);
        await DotNetCli.CreateProject(path, multiTarget: multiTarget);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");

        _ = await DotNetCli.BuildProject(path);

        var packages = new Matcher().AddInclude("**/bin/Debug/*.nupkg");
        Assert.NotEmpty(packages.GetResultsInFullPath(path));

        // act
        _ = await DotNetCli.Clean(path, ("GeneratePackageOnBuild", "true"));

        // assert
        Assert.Empty(packages.GetResultsInFullPath(path));
    }

    [Fact]
    public static async Task MinVerDoesNotRunWhenPackagesAreNotGeneratedOnBuild()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);

        // act
        var (standardOutput, _) = await DotNetCli.Clean(
            path, ("GeneratePackageOnBuild", "false"), ("MinVerVerbosity", "diagnostic"));

        // assert
        Assert.DoesNotContain("minver:", standardOutput, StringComparison.OrdinalIgnoreCase);
    }
}
