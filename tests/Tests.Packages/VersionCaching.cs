using System.Globalization;
using System.Reflection;
using Fixtures;
using Tests.Packages.Fixtures;
using Xunit;

namespace Tests.Packages;

public class VersionCaching
{
    [Fact]
    public async Task VersionIsCalculatedOnceInMultiProcessBuild()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await CreateTaggedSolution(path);

        // act
        var (actual, standardOutput, _) = await DotNetCli.BuildMultiProcess(path);

        // assert
        AssertCalculatedOnce(actual, standardOutput);
    }

    [Fact]
    public async Task VersionIsCalculatedOnceInMultiThreadedBuild()
    {
        var sdkVersion = await DotNetCli.GetVersionForTestProjects();
        if (int.Parse(sdkVersion.Split('.')[0], CultureInfo.InvariantCulture) < 11)
        {
            Assert.Skip($"SDK {sdkVersion} does not support multithreaded builds");
        }

        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await CreateTaggedSolution(path);

        // act
        var (actual, standardOutput, _) = await DotNetCli.BuildMultiThreaded(path);

        // assert
        AssertCalculatedOnce(actual, standardOutput);
    }

    [Fact]
    public async Task EachRepoHasItsOwnVersion()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateSolution(path, ["project0", "project1",]);

        var project0 = Path.Combine(path, "project0");
        await Git.InitAsync(project0);
        await Git.CommitAsync(project0);
        await Git.TagAsync(project0, "2.3.4");

        var project1 = Path.Combine(path, "project1");
        await Git.InitAsync(project1);
        await Git.CommitAsync(project1);
        await Git.TagAsync(project1, "5.6.7");

        // act
        var (actual, _, _) = await DotNetCli.Build(path);

        // assert
        Assert.Collection(
            actual,
            package => Assert.Equal(Package.WithVersion(2, 3, 4), package),
            package => Assert.Equal(Package.WithVersion(5, 6, 7), package));
    }

    private static async Task CreateTaggedSolution(string path)
    {
        // each project references the previous project, so the projects are versioned sequentially
        await DotNetCli.CreateSolution(path, ["project0", "project1", "project2", "project3",]);

        await Git.InitAsync(path);
        await Git.CommitAsync(path);
        await Git.TagAsync(path, "2.3.4");
    }

    private static void AssertCalculatedOnce(List<Package> actual, string standardOutput)
    {
        var versionCalculations = standardOutput
            .ToNonEmptyLines()
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("MinVer: Calculated version ", StringComparison.OrdinalIgnoreCase));

        _ = Assert.Single(versionCalculations);

        Assert.Equal(4, actual.Count);
        Assert.All(actual, package => Assert.Equal(Package.WithVersion(2, 3, 4), package));
    }
}
