using System.Reflection;
using MinVerTests.Infra;
using SimpleExec;
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

        var expected = Package.WithVersion(2, 3, 5, ["preview", "0",], 1);

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

        // act
        var sdkException = await Record.ExceptionAsync(() => Sdk.BuildProject(path, envVars: envVars));
        var cliException = await Record.ExceptionAsync(() => MinVerCli.ReadAsync(path, envVars: envVars));

        // assert
        Assert.Contains("MINVER1008: MinVerDefaultPreReleasePhase is no longer available", Assert.IsType<ExitCodeReadException>(sdkException).StandardOutput, StringComparison.Ordinal);
        Assert.Contains("MinVerDefaultPreReleasePhase is no longer available", Assert.IsType<ExitCodeReadException>(cliException).StandardError, StringComparison.Ordinal);
    }
}
