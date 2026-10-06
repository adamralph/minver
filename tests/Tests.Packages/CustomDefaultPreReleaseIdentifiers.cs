using System.Reflection;
using Fixtures;
using SimpleExec;
using Tests.Packages.Fixtures;
using Xunit;

namespace Tests.Packages;

public static class CustomDefaultPreReleaseIdentifiers
{
    [Fact]
    public static async Task HasCustomDefaultPreReleaseIdentifiers()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);

        await Git.InitAsync(path);
        await Git.CommitAsync(path);
        await Git.TagAsync(path, "2.3.4");
        await Git.CommitAsync(path);

        var envVars = ("MinVerDefaultPreReleaseIdentifiers".ToAltCase(), "preview.0");

        var expected = Package.WithVersion(2, 3, 5, ["preview", "0",], 1);

        // act
        var (actual, _, _) = await DotNetCli.BuildProject(path, envVars: envVars);
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
        await DotNetCli.CreateProject(path);

        await Git.InitAsync(path);
        await Git.CommitAsync(path);
        await Git.TagAsync(path, "2.3.4");
        await Git.CommitAsync(path);

        var envVars = ("MinVerDefaultPreReleasePhase".ToAltCase(), "preview");

        // act
        var sdkException = await Record.ExceptionAsync(() => DotNetCli.BuildProject(path, envVars: envVars));
        var cliException = await Record.ExceptionAsync(() => MinVerCli.ReadAsync(path, envVars: envVars));

        // assert
        Assert.Contains("MINVER1008: MinVerDefaultPreReleasePhase is no longer available", Assert.IsType<ExitCodeReadException>(sdkException).StandardOutput, StringComparison.Ordinal);
        Assert.Contains("MinVerDefaultPreReleasePhase is no longer available", Assert.IsType<ExitCodeReadException>(cliException).StandardError, StringComparison.Ordinal);
    }
}
