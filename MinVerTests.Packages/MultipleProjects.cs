using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public class MultipleProjects
{
    [RestrictedFact(
        new[] { "^3.*", "^5.*", "^6.*", },
        new[] { "OSX", },
        "With an SDK less than what's being used to run this test, or on macOS, there is sometimes a 15 minute delay after the `dotnet build` command")]
    public async Task MultipleTagPrefixes()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();

        await Sdk.CreateSolution(path, new[] { "project0", "project1", "project2", "project3", });

        var props =
            $@"<Project>

{"  "}<PropertyGroup>
{"    "}<MinVerTagPrefix>v</MinVerTagPrefix>
{"  "}</PropertyGroup>

</Project>
";

        await File.WriteAllTextAsync(Path.Combine(path, "project1", "Directory.Build.props"), props);
        await File.WriteAllTextAsync(Path.Combine(path, "project3", "Directory.Build.props"), props);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Tag(path, "v5.6.7");

        var expected0 = Package.WithVersion(2, 3, 4);
        var expected1 = Package.WithVersion(5, 6, 7);
        var expected2 = Package.WithVersion(2, 3, 4);
        var expected3 = Package.WithVersion(5, 6, 7);

        // act
        var (actual, standardOutput, _) = await Sdk.Build(path);

        // assert
        Assert.NotNull(standardOutput);

        var versionCalculations = standardOutput
            .Split(new[] { '\r', '\n', }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("MinVer: Calculated version ", StringComparison.OrdinalIgnoreCase));

        Assert.Collection(
            versionCalculations,
            message => Assert.Equal("MinVer: Calculated version 2.3.4.", message),
            message => Assert.Equal("MinVer: Calculated version 5.6.7.", message),
            message => Assert.Equal("MinVer: Calculated version 2.3.4.", message),
            message => Assert.Equal("MinVer: Calculated version 5.6.7.", message));

        Assert.Collection(
            actual,
            package => Assert.Equal(expected0, package),
            package => Assert.Equal(expected1, package),
            package => Assert.Equal(expected2, package),
            package => Assert.Equal(expected3, package));
    }
}
