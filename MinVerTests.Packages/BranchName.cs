using MinVerTests.Infra;
using System.Reflection;
using Xunit;

namespace MinVerTests.Packages;

public static class BranchName
{
    [Fact]
    public static async Task IncludesBranchName()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);

        // Create a branch
        await Git.BranchAsync(path, "feature/billing");
        await Git.Commit(path);

        // Creating a tag to have a known version
        await Git.Tag(path, "1.2.3");

        // Make another commit after the tag
        await Git.Commit(path);

        // act
        var envVars = ("MinVerIncludeBranchName".ToAltCase(), "true");
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, args: "--include-branch-name");

        // assert
        Assert.Equal("1.2.4-feature-billing.alpha.0.1", actual?.InformationalVersion);
        Assert.Equal("1.2.4-feature-billing.alpha.0.1", cliStandardOutput.Trim());
    }

    [Fact]
    public static async Task IgnoresPrereleaseIdentifiersWithBranchName()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);

        // Create a branch
        await Git.BranchAsync(path, "develop");
        await Git.Commit(path);

        // Creating a tag to have a known version
        await Git.Tag(path, "2.0.0");

        // Make another commit after the tag
        await Git.Commit(path);

        // act - ignore pre-release identifiers but include branch name
        var envVars = new[] {
            ("MinVerIgnorePreReleaseIdentifiers".ToAltCase(), "true"),
            ("MinVerIncludeBranchName".ToAltCase(), "true")
        };

        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, args: "--ignore-pre-release-identifiers --include-branch-name");

        // assert - expect branch name in version without default pre-release identifiers
        Assert.Equal("2.0.1-develop.1", actual?.InformationalVersion);
        Assert.Equal("2.0.1-develop.1", cliStandardOutput.Trim());
    }
}
