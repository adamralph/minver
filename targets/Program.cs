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
                Environment.SetEnvironmentVariable("MINVER_TAG_PREFIX", "v", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("NoPackageAnalysis", "true", EnvironmentVariableTarget.Process);

                var source = Path.GetFullPath("./MinVer/bin/Release/");
                var version = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

                var path = FileSystem.GetScenarioDirectory("package");
                await Git.EnsureRepositoryWithACommit(path);

                await RunAsync("git", "tag v1.2.3", path);

                await RunAsync("git", "commit --allow-empty -m '.'", path);

                await RunAsync("dotnet", "new classlib", path);
                await RunAsync("dotnet", $"add package MinVer --version {version} --source {source} --package-directory packages", path);
                await RunAsync("dotnet", $"restore --source {source} --packages packages", path);

                DeletePackages();

                await RunAsync("dotnet", "build --no-restore", path);
                await RunAsync("dotnet", "pack --no-build", path);

                var package = Directory.EnumerateFiles(path, "*.nupkg", new EnumerationOptions { RecurseSubdirectories = true }).First();
                var expected = "1.2.4-alpha.0.1.nupkg";
                if (!package.Contains(expected))
                {
                    throw new Exception($"'{package}' does not contain '{expected}'.");
                }

                Environment.SetEnvironmentVariable("MINVER_BUILD_METADATA", "build.42", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("MINVER_VERBOSE", "true", EnvironmentVariableTarget.Process);

                // normally set via an MSBuild property
                Environment.SetEnvironmentVariable("MinVerMajorMinor", "2.0", EnvironmentVariableTarget.Process);

                DeletePackages();

                await RunAsync("dotnet", "build --no-restore", path);
                await RunAsync("dotnet", "pack --no-build", path);

                package = Directory.EnumerateFiles(path, "*.nupkg", new EnumerationOptions { RecurseSubdirectories = true }).First();
                expected = "2.0.0-alpha.0.1.nupkg";
                if (!package.Contains(expected))
                {
                    throw new Exception($"'{package}' does not contain '{expected}'.");
                }

                Environment.SetEnvironmentVariable("MINVER_VERSION", "3.0.0-beta.2+build.52", EnvironmentVariableTarget.Process);

                DeletePackages();

                await RunAsync("dotnet", "build --no-restore", path);
                await RunAsync("dotnet", "pack --no-build", path);

                package = Directory.EnumerateFiles(path, "*.nupkg", new EnumerationOptions { RecurseSubdirectories = true }).First();
                expected = "3.0.0-beta.2.nupkg";
                if (!package.Contains(expected))
                {
                    throw new Exception($"'{package}' does not contain '{expected}'.");
                }

                void DeletePackages()
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*.nupkg", new EnumerationOptions { RecurseSubdirectories = true }))
                    {
                        File.Delete(file);
                    }
                }
            });

        return RunTargetsAsync(args);
    }
}
