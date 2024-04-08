using System.Globalization;
using System.Reflection;
using MinVerTests.Infra;
using MinVerTests.Packages.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class NoGit
{
    [WindowsFact]
    public static async Task GitIsNotInPath()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await Sdk.CreateProject(path);
        await Git.Init(path);
        await Git.Commit(path);

        var pathEnvVar = Environment.GetEnvironmentVariable("PATH") ?? "";
        pathEnvVar = pathEnvVar.Replace("git", "not-git", true, CultureInfo.InvariantCulture);

        var sdkExitCode = 0;
        var cliExitCode = 0;

        // act
        var (_, sdkStandardOutput, _) = await Sdk.BuildProject(
            path,
            exitCode =>
            {
                sdkExitCode = exitCode;
                return true;
            },
            ("PATH", pathEnvVar));

        var (_, cliStandardError) = await MinVerCli.ReadAsync(
            path,
            handleExitCode: exitCode =>
            {
                cliExitCode = exitCode;
                return true;
            },
            envVars: ("PATH", pathEnvVar));

        // assert
        Assert.NotEqual(0, sdkExitCode);
        Assert.Contains("MINVER1007", sdkStandardOutput, StringComparison.Ordinal);

        Assert.NotEqual(0, cliExitCode);
        Assert.Contains("\"git\" is not present in PATH.", cliStandardError, StringComparison.Ordinal);
    }
}
