using System;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class OptionMasking
{
    [Theory]
    [InlineData("patch")]
    public static async Task AutoIncrementBackToDefault(string value)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        FileSystem.EnsureEmptyDirectory(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);

        var envVars = ("MinVerAutoIncrement".ToAltCase(), "minor");
        var args = $"--auto-increment {value}";

        var expected = Package.WithVersion(2, 3, 5, new[] { "alpha", "0", }, 1);

        // act
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, args: args, envVars: envVars);

        // assert
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Theory]
    [InlineData("\"\"")]

    public static async Task BuildMetadataBackToDefault(string value)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        FileSystem.EnsureEmptyDirectory(path);

        var envVars = ("MinVerBuildMetadata", "build.123");
        var args = $"--build-metadata {value}";

        var expected = Package.WithVersion(0, 0, 0, new[] { "alpha", "0", });

        // act
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, args: args, envVars: envVars);

        // assert
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Theory]
    [InlineData("alpha.0")]
    public static async Task DefaultPreReleaseIdentifiersBackToDefault(string value)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        FileSystem.EnsureEmptyDirectory(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);

        var envVars = ("MinVerDefaultPreReleaseIdentifiers".ToAltCase(), "preview.0");
        var args = $"--default-pre-release-identifiers {value}";

        var expected = Package.WithVersion(2, 3, 5, new[] { "alpha", "0", }, 1);

        // act
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, args: args, envVars: envVars);

        // assert
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Theory]
    [InlineData("alpha")]
    public static async Task DefaultPreReleasePhaseBackToDefault(string value)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        FileSystem.EnsureEmptyDirectory(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);

        var envVars = ("MinVerDefaultPreReleasePhase".ToAltCase(), "preview");
        var args = $"--default-pre-release-phase {value}";

        var expected = Package.WithVersion(2, 3, 5, new[] { "alpha", "0", }, 1);

        // act
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, args: args, envVars: envVars);

        // assert
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Theory]
    [InlineData("0.0")]
    public static async Task MinimumMajorMinorBackToDefault(string value)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        FileSystem.EnsureEmptyDirectory(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");

        var envVars = ("MinVerMinimumMajorMinor".ToAltCase(), "3.0");
        var args = $"--minimum-major-minor {value}";

        var expected = Package.WithVersion(2, 3, 4);

        // act
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, args: args, envVars: envVars);

        // assert
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Theory]
    [InlineData("\"\"")]
    public static async Task TagPrefixBackToDefault(string value)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        FileSystem.EnsureEmptyDirectory(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4-alpha.5");

        var envVars = ("MinVerTagPrefix", "v.");
        var args = $"--tag-prefix {value}";

        var expected = Package.WithVersion(2, 3, 4, new[] { "alpha", "5", });

        // act
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, args: args, envVars: envVars);

        // assert
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Theory]
    [InlineData("info")]
    public static async Task VerbosityBackToDefault(string value)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        FileSystem.EnsureEmptyDirectory(path);

        var envVars = ("MinVerVerbosity", "error");
        var args = $"--verbosity {value}";

        // act
        var (_, cliStandardError) = await MinVerCli.ReadAsync(path, args: args, envVars: envVars);

        // assert
        Assert.Contains("MinVer:", cliStandardError, StringComparison.Ordinal);
    }
}
