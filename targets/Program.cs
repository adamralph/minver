using static Bullseye.Targets;
using static SimpleExec.Command;

Target("restore", () => RunAsync("dotnet", "restore"));

Target("format", dependsOn: ["restore",], () => RunAsync("dotnet", "format --verify-no-changes --no-restore"));

Target("build", dependsOn: ["restore",], () => RunAsync("dotnet", "build --configuration Release --no-restore"));

Target("pack", dependsOn: ["build",], () => RunAsync("dotnet", "pack --configuration Release --output artifacts --no-build"));

Target(
    "test-lib",
    "test the MinVer.Lib library",
    dependsOn: ["build",],
    () => RunAsync("dotnet", $"test --project ./tests/Tests.Lib --configuration Release --no-build"));

Target(
    "test-packages",
    "test the MinVer package and the minver-cli console app",
    dependsOn: ["pack",],
    () => RunAsync("dotnet", "test --project ./tests/Tests.Packages --configuration Release --no-build"));

Target("default", dependsOn: ["format", "test-lib", "test-packages",]);

await RunTargetsAndExitAsync(args, ex => ex is SimpleExec.ExitCodeException);
