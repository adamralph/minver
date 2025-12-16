using System.Reflection;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class SourceLink
{
    [Fact]
    public static async Task HasCommitSha()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        var sha = (await Git.GetCommitShas(path)).Single();

        var buildMetadata = "build.123";
        (string, string)[] envVars = [
            ("IncludeSourceRevisionInInformationalVersion", "true"),
            ("MinVerBuildMetadata", buildMetadata),
        ];

        var expected = Package.WithVersion(0, 0, 0, ["alpha", "0",], 0, "build.123", $".{sha}");

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal($"{expected.Version}+{buildMetadata}", cliStandardOutput.Trim());
    }
}
