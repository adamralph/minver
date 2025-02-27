using MinVerTests.Infra;
using System.Reflection;
using Xunit;

namespace MinVerTests.Packages;

public static class IgnorePreReleaseIdentifiers
{
    [Fact]
    public static async Task HasIgnorePreReleaseIdentifiers()
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
            ("MinVerIgnorePreReleaseIdentifiers".ToAltCase(), "true")
        };

        var expected = Package.WithVersion(2, 3, 5, [], 1);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Fact]
    public static async Task HasIgnorePreReleaseIdentifiersAndIgnoreHeight()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);
        await Git.Commit(path);
        await Git.Commit(path);

        var envVars = new[]
        {
            ("MinVerIgnorePreReleaseIdentifiers".ToAltCase(), "true"),
            ("MinVerIgnoreHeight".ToAltCase(), "true"),
        };

        var expected = Package.WithVersion(2, 3, 7);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Fact]
    public static async Task AutoIncrementWorks()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);
        await Git.Commit(path);
        await Git.Commit(path);

        (string, string)[] envVars = [];


        var expected = Package.WithVersion(2, 3, 7, ["alpha", "0"], 3);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Fact]
    public static async Task HasIgnorePreReleaseIdentifiersAndHasEmptyCustomDefaultPreReleaseIdentifiers()
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
            ("MinVerIgnorePreReleaseIdentifiers".ToAltCase(), "true"),
            ("MinVerDefaultPreReleaseIdentifiers".ToAltCase(), string.Empty)
        };

        var expected = Package.WithVersion(2, 3, 5, [], 1);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Fact]
    public static async Task HasIgnorePreReleaseIdentifiersAndHasCustomDefaultPreReleaseIdentifiers()
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
            ("MinVerIgnorePreReleaseIdentifiers".ToAltCase(), "true"),
            ("MinVerDefaultPreReleaseIdentifiers".ToAltCase(), "preview.0")
        };

        var expected = Package.WithVersion(2, 3, 5, ["preview", "0",], 1);

        // act
        var (actual, sdkStandardOutput, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, cliStandardError) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Contains("MINVER1009", sdkStandardOutput, StringComparison.Ordinal);

        Assert.Equal(expected.Version, cliStandardOutput.Trim());
        Assert.Contains("MinVerIgnorePreReleaseIdentifiers is not compatbile with MinVerDefaultPreReleaseIdentifiers.", cliStandardError, StringComparison.Ordinal);
    }
}
