using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using MinVerTests.Infra;

using static Bullseye.Targets;
using static MinVerTests.Infra.FileSystem;
using static MinVerTests.Infra.Git;
using static SimpleExec.Command;

internal static class Program
{
    private static int buildNumber;

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
            () => RunAsync("dotnet", $"publish ./MinVer/MinVer.csproj --configuration Release --no-build"));

        Target(
            "pack",
            DependsOn("publish"),
            ForEach("./MinVer/MinVer.csproj", "./minver-cli/minver-cli.csproj"),
            project => RunAsync("dotnet", $"pack {project} --configuration Release --no-build"));

        string testRepo = default;

        Target(
            "test-package-empty-repo",
            DependsOn("pack"),
            async () =>
            {
                // arrange
                Environment.SetEnvironmentVariable("MinVerTagPrefix", "v.", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("NoPackageAnalysis", "true", EnvironmentVariableTarget.Process);

                testRepo = FileSystem.GetScenarioDirectory("package");
                EnsureEmptyDirectory(testRepo);

                var source = Path.GetFullPath("./MinVer/bin/Release/");
                var version = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

                await RunAsync("dotnet", "new classlib", testRepo);
                await RunAsync("dotnet", $"add package MinVer --source {source} --version {version} --package-directory packages", testRepo);
                await RunAsync("dotnet", $"restore --source {source} --packages packages", testRepo);

                // act
                await CleanAndPack(testRepo);

                // assert
                AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", testRepo);
            });

        Target(
            "test-package-commit",
            DependsOn("test-package-empty-repo"),
            async () =>
            {
                // arrange
                Repository.Init(testRepo);

                using (var repo = new Repository(testRepo))
                {
                    repo.PrepareForCommits();
                    Commit(testRepo);

                    // act
                    await CleanAndPack(testRepo);

                    // assert
                    AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", testRepo);
                }
            });

        Target(
            "test-package-tag",
            DependsOn("test-package-commit"),
            async () =>
            {
                using (var repo = new Repository(testRepo))
                {
                    // arrange
                    repo.ApplyTag("v.1.2.3+foo");

                    // act
                    await CleanAndPack(testRepo);

                    // assert
                    AssertPackageFileNameContains("1.2.3.nupkg", testRepo);
                }
            });

        Target(
            "test-package-commit-after-tag",
            DependsOn("test-package-tag"),
            async () =>
            {
                using (var repo = new Repository(testRepo))
                {
                    // arrange
                    Commit(testRepo);

                    // act
                    await CleanAndPack(testRepo);

                    // assert
                    AssertPackageFileNameContains("1.2.4-alpha.0.1.nupkg", testRepo);
                }
            });

        Target(
            "test-package-major-minor",
            DependsOn("test-package-commit-after-tag"),
            async () =>
            {
                using (var repo = new Repository(testRepo))
                {
                    // arrange
                    Environment.SetEnvironmentVariable("MinVerMajorMinor", "2.0", EnvironmentVariableTarget.Process);

                    // act
                    Environment.SetEnvironmentVariable("MinVerVerbosity", "detailed", EnvironmentVariableTarget.Process);
                    await CleanAndPack(testRepo);

                    // assert
                    AssertPackageFileNameContains("2.0.0-alpha.0.1.nupkg", testRepo);
                }
            });

        Target("test-package", DependsOn("test-package-major-minor"));

        return RunTargetsAsync(args);
    }

    private static async Task CleanAndPack(string path)
    {
        Environment.SetEnvironmentVariable("MinVerBuildMetadata", $"build.{buildNumber++}", EnvironmentVariableTarget.Process);

        foreach (var file in Directory.EnumerateFiles(path, "*.nupkg", new EnumerationOptions { RecurseSubdirectories = true }))
        {
            File.Delete(file);
        }

        await RunAsync("dotnet", "build --no-restore", path);
        await RunAsync("dotnet", "pack --no-build", path);
    }

    private static void AssertPackageFileNameContains(string expected, string path)
    {
        var fileName = Directory.EnumerateFiles(path, "*.nupkg", new EnumerationOptions { RecurseSubdirectories = true })
            .First();

        if (!fileName.Contains(expected))
        {
            throw new Exception($"'{fileName}' does not contain '{expected}'.");
        }
    }
}
