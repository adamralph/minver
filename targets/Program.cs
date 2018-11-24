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
    private static readonly string testPackageBaseOutput = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(typeof(Program).Assembly.CodeBase).Path));

    private static int buildNumber = 1;

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
            "test-package-no-repo",
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

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-no-repo");

                // act
                await CleanAndPack(testRepo, output);

                // assert
                AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", output);
            });

        Target(
            "test-package-no-commits",
            DependsOn("test-package-no-repo"),
            async () =>
            {
                // arrange
                Repository.Init(testRepo);

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-no-commits");

                // act
                await CleanAndPack(testRepo, output);

                // assert
                AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", output);
            });

        Target(
            "test-package-commit",
            DependsOn("test-package-no-commits"),
            async () =>
            {
                using (var repo = new Repository(testRepo))
                {
                    // assert
                    repo.PrepareForCommits();
                    Commit(testRepo);

                    var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-commit");

                    // act
                    await CleanAndPack(testRepo, output);

                    // assert
                    AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", output);
                }
            });

        Target(
            "test-package-non-version-tag",
            DependsOn("test-package-commit"),
            async () =>
            {
                using (var repo = new Repository(testRepo))
                {
                    // arrange
                    repo.ApplyTag("foo");

                    var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-non-version-tag");

                    // act
                    Environment.SetEnvironmentVariable("MinVerVerbosity", "detailed", EnvironmentVariableTarget.Process);
                    await CleanAndPack(testRepo, output);
                    Environment.SetEnvironmentVariable("MinVerVerbosity", "normal", EnvironmentVariableTarget.Process);

                    // assert
                    AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", output);
                }
            });

        Target(
            "test-package-version-tag",
            DependsOn("test-package-non-version-tag"),
            async () =>
            {
                using (var repo = new Repository(testRepo))
                {
                    // arrange
                    repo.ApplyTag("v.1.2.3+foo");

                    var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-version-tag");

                    // act
                    await CleanAndPack(testRepo, output);

                    // assert
                    AssertPackageFileNameContains("1.2.3.nupkg", output);
                }
            });

        Target(
            "test-package-commit-after-tag",
            DependsOn("test-package-version-tag"),
            async () =>
            {
                using (var repo = new Repository(testRepo))
                {
                    // arrange
                    Commit(testRepo);

                    var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-commit-after-tag");

                    // act
                    await CleanAndPack(testRepo, output);

                    // assert
                    AssertPackageFileNameContains("1.2.4-alpha.0.1.nupkg", output);
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

                    var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-major-minor");

                    // act
                    Environment.SetEnvironmentVariable("MinVerVerbosity", "detailed", EnvironmentVariableTarget.Process);
                    await CleanAndPack(testRepo, output);

                    // assert
                    AssertPackageFileNameContains("2.0.0-alpha.0.1.nupkg", output);
                }
            });

        Target("test-package", DependsOn("test-package-major-minor"));

        return RunTargetsAsync(args);
    }

    private static async Task CleanAndPack(string path, string output)
    {
        Environment.SetEnvironmentVariable("MinVerBuildMetadata", $"build.{buildNumber++}", EnvironmentVariableTarget.Process);

        EnsureEmptyDirectory(output);

        await RunAsync("dotnet", "build --no-restore", path);
        await RunAsync("dotnet", $"pack --no-build --output {output}", path);
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
