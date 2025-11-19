using MinVerTests.Infra;
using static Bullseye.Targets;
using static SimpleExec.Command;

var testFx = Environment.GetEnvironmentVariable("MINVER_TESTS_FRAMEWORK") ?? "net8.0";
var testLoggerArgs = new List<string> { "--logger", "\"console;verbosity=normal\"", };

var isGha = Environment.GetEnvironmentVariable("GITHUB_ACTIONS")?.ToUpperInvariant() == "TRUE";
if (isGha)
{
    testLoggerArgs.AddRange(["--logger", "GitHubActions",]);
}

Target("format", () => RunAsync("dotnet", "format --verify-no-changes"));

Target(
    "gha-build-msbuild-caching",
    "in GitHub Actions, build MSBuild.Caching project first, to avoid intermittent failures",
    () => !isGha
        ? Console.Out.WriteLineAsync("Skipping because not running inside GitHub Actions")
        : RunAsync("dotnet", "build ./MSBuild.Caching --configuration Release --nologo"));

Target("build", dependsOn: ["gha-build-msbuild-caching"], () => RunAsync("dotnet", "build --configuration Release --nologo"));

Target(
    "test-lib",
    "test the MinVer.Lib library",
    dependsOn: ["build"],
    () => RunAsync("dotnet", ["test", "./MinVerTests.Lib", "--framework", testFx, "--configuration", "Release", "--no-build", "--nologo", .. testLoggerArgs,]));

Target(
    "test-packages",
    "test the MinVer package and the minver-cli console app",
    dependsOn: ["build"],
    () => RunAsync("dotnet", ["test", "./MinVerTests.Packages", "--configuration", "Release", "--no-build", "--nologo", .. testLoggerArgs,]));

Target(
    "eyeball-minver-logs",
    "build a test solution with the MinVer package to eyeball the diagnostic logs",
    dependsOn: ["build"],
    async () =>
    {
        var path = TestDirectory.Get("MinVer.Targets", "eyeball-minver-logs");

        await Sdk.CreateSolution(path, ["project0", "project1",], "Release");

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "v.2.3.4-alpha.5");
        await Git.Commit(path);

        await RunAsync(
            "dotnet",
            "build --no-restore --nologo -maxCpuCount:1",
            path,
            configureEnvironment: env =>
            {
                env.Add("MinVerBuildMetadata", "build.6");
                env.Add("MinVerTagPrefix", "v.");
                env.Add("MinVerVerbosity", "diagnostic");
            });
    });

Target(
    "eyeball-minver-cli-logs",
    "run the minver-cli console app on a test directory to eyeball the trace logs",
    dependsOn: ["build"],
    async () =>
    {
        var path = TestDirectory.Get("MinVer.Targets", "eyeball-minver-cli-logs");

        FileSystem.EnsureEmptyDirectory(path);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "v.2.3.4-alpha.5");
        await Git.Commit(path);

        await RunAsync(
            "dotnet",
            $"exec {MinVerCli.GetPath("Release")} {path}",
            configureEnvironment: env =>
            {
                env.Add("MinVerBuildMetadata", "build.6");
                env.Add("MinVerTagPrefix", "v.");
                env.Add("MinVerVerbosity", "trace");
            });
    });

Target("eyeball-logs", dependsOn: ["eyeball-minver-logs", "eyeball-minver-cli-logs"]);

Target("default", dependsOn: ["format", "test-lib", "test-packages", "eyeball-logs"]);

await RunTargetsAndExitAsync(args, ex => ex is SimpleExec.ExitCodeException);
