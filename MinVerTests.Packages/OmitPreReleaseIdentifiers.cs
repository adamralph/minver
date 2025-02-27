using MinVerTests.Infra;
using System.Reflection;
using Xunit;

namespace MinVerTests.Packages;

public static class OmitPreReleaseIdentifiers
{
    [Fact]
    public static async Task HasOmitPreReleaseIdentifiers()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");
        await Git.Commit(path);

        var envVars = ("MinVerOmitPreReleaseIdentifiers".ToAltCase(), "true");

        var expected = Package.WithVersion(2, 3, 5);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Fact]
    public static async Task HasOmitPreReleaseIdentifiersAndHasEmptyCustomDefaultPreReleaseIdentifiers()
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
            ("MinVerOmitPreReleaseIdentifiers".ToAltCase(), "true"),
            ("MinVerDefaultPreReleaseIdentifiers".ToAltCase(), string.Empty)
        };

        var expected = Package.WithVersion(2, 3, 5);

        // act
        var (actual, _, _) = await Sdk.BuildProject(path, envVars: envVars);
        var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path, envVars: envVars);

        // assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Version, cliStandardOutput.Trim());
    }

    [Fact]
    public static async Task HasOmitPreReleaseIdentifiersAndHasCustomDefaultPreReleaseIdentifiers()
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
            ("MinVerOmitPreReleaseIdentifiers".ToAltCase(), "true"),
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
        Assert.Contains("MinVerOmitPreReleaseIdentifiers is not compatbile with MinVerDefaultPreReleaseIdentifiers.", cliStandardError, StringComparison.Ordinal);
    }
}
