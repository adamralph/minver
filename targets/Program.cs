using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Bullseye.Targets;
using static MinVerTests.Infra.FileSystem;
using static MinVerTests.Infra.Git;
using static SimpleExec.Command;

internal static class Program
{
    private static readonly string testPackageBaseOutput = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(typeof(Program).Assembly.CodeBase).Path));

    private static int buildNumber = 1;

    public static async Task Main(string[] args)
    {
        Target("default", DependsOn("test-api", "test-package"));

        Target("build", () => RunAsync("dotnet", "build --configuration Release --nologo --verbosity quiet"));

        Target(
            "test-api",
            DependsOn("build"),
            () => RunAsync("dotnet", "test --configuration Release --no-build --nologo"));

        Target(
            "publish",
            DependsOn("build"),
            () => RunAsync("dotnet", "publish ./MinVer/MinVer.csproj --configuration Release --no-build --nologo"));

        Target(
            "pack",
            DependsOn("publish"),
            ForEach("./MinVer/MinVer.csproj", "./minver-cli/minver-cli.csproj"),
            project => RunAsync("dotnet", $"pack {project} --configuration Release --no-build --nologo"));

        string testProject = default;

        Target(
            "create-test-project",
            DependsOn("pack"),
            async () =>
            {
                testProject = GetScenarioDirectory("package");
                EnsureEmptyDirectory(testProject);

                var source = Path.GetFullPath("./MinVer/bin/Release/");
                var version = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

                await RunAsync("dotnet", "new classlib", testProject);
                await RunAsync("dotnet", $"add package MinVer --source {source} --version {version} --package-directory packages", testProject);
                await RunAsync("dotnet", $"restore --source {source} --packages packages", testProject);
            });

        Target(
            "test-package-no-repo",
            DependsOn("create-test-project"),
            async () =>
            {
                // arrange
                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-no-repo");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", output);
            });

        Target(
            "test-package-no-commits",
            DependsOn("test-package-no-repo"),
            async () =>
            {
                // arrange
                Init(testProject);

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-no-commits");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", output);
            });

        Target(
            "test-package-commit",
            DependsOn("test-package-no-commits"),
            async () =>
            {
                // assert
                PrepareForCommits(testProject);
                Commit(testProject);

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-commit");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", output);
            });

        Target(
            "test-package-non-version-tag",
            DependsOn("test-package-commit"),
            async () =>
            {
                // arrange
                Tag(testProject, "foo");

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-non-version-tag");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertPackageFileNameContains("0.0.0-alpha.0.nupkg", output);
            });

        Target(
            "test-package-version-tag",
            DependsOn("test-package-non-version-tag"),
            async () =>
            {
                // arrange
                Tag(testProject, "v.1.2.3+foo");

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-version-tag");

                // act
                await CleanAndPack(testProject, output, "normal", env => env.Add("MinVerTagPrefix", "v."));

                // assert
                AssertPackageFileNameContains("1.2.3.nupkg", output);
            });

        Target(
            "test-package-commit-after-tag",
            DependsOn("test-package-version-tag"),
            async () =>
            {
                // arrange
                Tag(testProject, "1.2.3+foo");
                Commit(testProject);

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-commit-after-tag");

                // act
                await CleanAndPack(testProject, output, "detailed");

                // assert
                AssertPackageFileNameContains("1.2.4-alpha.0.1.nupkg", output);
            });

        Target(
            "test-package-non-default-auto-increment",
            DependsOn("test-package-commit-after-tag"),
            async () =>
            {
                // arrange
                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-non-default-auto-increment");

                // act
                await CleanAndPack(testProject, output, "diagnostic", env => env.Add("MinVerAutoIncrement", "minor"));

                // assert
                AssertPackageFileNameContains("1.3.0-alpha.0.1.nupkg", output);
            });

        Target(
            "test-package-annotated-tag",
            DependsOn("test-package-non-default-auto-increment"),
            async () =>
            {
                // arrange
                AnnotatedTag(testProject, "1.4.0", "foo");

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-annotated-tag");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertPackageFileNameContains("1.4.0.nupkg", output);
            });

        Target(
            "test-package-minimum-major-minor-on-tag",
            DependsOn("test-package-annotated-tag"),
            async () =>
            {
                // arrange
                Commit(testProject);
                Tag(testProject, "1.5.0");

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-minimum-major-minor-on-tag");

                // act
                await CleanAndPack(testProject, output, "diagnostic", env => env.Add("MinVerMinimumMajorMinor", "2.0"));

                // assert
                AssertPackageFileNameContains("2.0.0-alpha.0.nupkg", output);
            });

        Target(
            "test-package-minimum-major-minor-after-tag",
            DependsOn("test-package-minimum-major-minor-on-tag"),
            async () =>
            {
                // arrange
                Commit(testProject);

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-minimum-major-minor-after-tag");

                // act
                await CleanAndPack(testProject, output, "diagnostic", env => env.Add("MinVerMinimumMajorMinor", "2.0"));

                // assert
                AssertPackageFileNameContains("2.0.0-alpha.0.1.nupkg", output);
            });

        Target(
            "test-package-default-pre-release-phase",
            DependsOn("test-package-minimum-major-minor-after-tag"),
            async () =>
            {
                // arrange
                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-default-pre-release-phase");

                // act
                await CleanAndPack(testProject, output, "diagnostic", env => env.Add("MinVerDefaultPreReleasePhase", "preview"));

                // assert
                AssertPackageFileNameContains("1.5.1-preview.0.1.nupkg", output);
            });

        Target(
            "test-package-version-override",
            DependsOn("test-package-default-pre-release-phase"),
            async () =>
            {
                // arrange
                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-version-override");

                // act
                await CleanAndPack(testProject, output, "diagnostic", env => env.Add("MinVerVersionOverride", "3.2.1-rc.4+build.5"));

                // assert
                AssertPackageFileNameContains("3.2.1-rc.4.nupkg", output);
            });

        Target("test-package", DependsOn("test-package-version-override"));

        await RunTargetsAndExitAsync(args);
    }

    private static async Task CleanAndPack(string path, string output, string verbosity, Action<IDictionary<string, string>> configureEnvironment = null)
    {
        EnsureEmptyDirectory(output);

        await RunAsync("dotnet", "build --no-restore --nologo", path, configureEnvironment: configureEnvironment);
        await RunAsync(
            "dotnet",
            $"pack --no-build --output {output} --nologo",
            path,
            configureEnvironment: env =>
            {
                configureEnvironment?.Invoke(env);
                env.Add("MinVerBuildMetadata", $"build.{buildNumber++}");
                env.Add("MinVerVerbosity", verbosity ?? "");
                env.Add("NoPackageAnalysis", "true");
            });
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
