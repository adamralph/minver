using System;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class CustomDefaultPreReleaseIdentifiers
{
    [Fact]
    public static async Task HasCustomDefaultPreReleaseIdentifiers()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);

        var envVars = ("MinVerDefaultPreReleaseIdentifiers".ToAltCase(), "preview.0");

        var expected = Package.WithVersion(2, 3, 5, new[] { "preview", "0", }, 1);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Fact]
    public static async Task HasCustomDefaultPreReleasePhase()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);

        var envVars = ("MinVerDefaultPreReleasePhase".ToAltCase(), "preview");

        var expected = Package.WithVersion(2, 3, 5, new[] { "preview", "0", }, 1);

        // act
        var (actual, sdkStandardOutput, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, cliStandardError) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Contains("MINVER1008", sdkStandardOutput, StringComparison.Ordinal);

        Assert.Equal(expected.Version, cliStandardOutput.Trim());
        Assert.Contains("MinVerDefaultPreReleasePhase is deprecated", cliStandardError, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task HasCustomDefaultPreReleaseIdentifiersAndCustomDefaultPreReleasePhase()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);

        var envVars = new[]
        {
            ("MinVerDefaultPreReleaseIdentifiers".ToAltCase(), "preview.0"),
            ("MinVerDefaultPreReleasePhase", "foo"),
        };

        var expected = Package.WithVersion(2, 3, 5, new[] { "preview", "0", }, 1);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }
}
