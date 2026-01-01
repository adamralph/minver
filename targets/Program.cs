using MinVerTests.Infra;
using static Bullseye.Targets;
using static SimpleExec.Command;

Target("format", () => RunAsync("dotnet", "format --verify-no-changes"));

Target("build", () => RunAsync("dotnet", "build --configuration Release"));

Target("pack", dependsOn: ["build",], () => RunAsync("dotnet", "pack --configuration Release --output artifacts --no-build"));

Target(
    "test-lib",
    "test the MinVer.Lib library",
    dependsOn: ["build",],
    () => RunAsync("dotnet", $"test --project ./MinVerTests.Lib --configuration Release --no-build"));

Target(
    "test-packages",
    "test the MinVer package and the minver-cli console app",
    dependsOn: ["pack",],
    () => RunAsync("dotnet", "test --project ./MinVerTests.Packages --configuration Release --no-build"));

Target(
    "eyeball-minver-logs",
    "build a test solution with the MinVer package to eyeball the diagnostic logs",
    dependsOn: ["pack",],
    async () =>
    {
        var path = TestDirectory.Get("MinVer.Targets", "eyeball-minver-logs");

        await Sdk.CreateSolution(path, ["project0", "project1",]);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "v.2.3.4-alpha.5");
        await Git.Commit(path);

        await RunAsync(
            "dotnet",
            "build --no-restore -maxCpuCount:1",
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
    dependsOn: ["build",],
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
            $"exec {await MinVerCli.GetPath("Release")} {path}",
            configureEnvironment: env =>
            {
                env.Add("MinVerBuildMetadata", "build.6");
                env.Add("MinVerTagPrefix", "v.");
                env.Add("MinVerVerbosity", "trace");
            });
    });

Target("eyeball-logs", dependsOn: ["eyeball-minver-logs", "eyeball-minver-cli-logs",]);

Target("default", dependsOn: ["format", "test-lib", "test-packages", "eyeball-logs",]);

await RunTargetsAndExitAsync(args, ex => ex is SimpleExec.ExitCodeException);
