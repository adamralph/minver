using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MinVerTests.Infra;
using static Bullseye.Targets;
using static SimpleExec.Command;

internal class Program
{
    public static Task Main(string[] args)
    {
        Target("default", DependsOn("test-api", "test-package"));

        Target("build", () => RunAsync("dotnet", "build MinVer.sln --configuration Release"));

        Target(
            "test-api",
            DependsOn("build"),
            () => RunAsync("dotnet", $"test ./MinVerTests/MinVerTests.csproj --configuration Release --no-build --verbosity=normal"));

        Target(
            "publish",
            DependsOn("build"),
            () => RunAsync("dotnet", $"publish ./MinVer.Cli/MinVer.Cli.csproj --configuration Release --no-build"));

        Target(
            "pack",
            DependsOn("publish"),
            () => RunAsync("dotnet", $"pack ./MinVer/MinVer.csproj --configuration Release --no-build"));

        Target(
            "test-package",
            DependsOn("pack"),
            async () =>
            {
                var source = Path.GetFullPath("./MinVer/bin/Release/");
                var version = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

                var path = FileSystem.GetScenarioDirectory("package");
                await Git.EnsureEmptyRepository(path);

                await RunAsync("dotnet", "new classlib", path);
                await RunAsync("dotnet", $"add package MinVer --version {version} --source {source} --package-directory packages", path);
                await RunAsync("dotnet", $"restore --source {source} --packages packages", path);
                await RunAsync("dotnet", "build --no-restore", path);
                await RunAsync("dotnet", "pack --no-build", path);

                var package = Directory.EnumerateFiles(path, "*.nupkg", new EnumerationOptions { RecurseSubdirectories = true }).First();
                var expected = "0.0.0-alpha.0";
                if (!package.Contains(expected))
                {
                    throw new Exception($"'{package}' does not contain '{expected}'.");
                }
            });

        return RunTargetsAsync(args);
    }
}
