using System;
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
            () => RunAsync("dotnet", "test --configuration Release --no-build --verbosity=normal --nologo"));

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
                Environment.SetEnvironmentVariable("NoPackageAnalysis", "true", EnvironmentVariableTarget.Process);

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
                Environment.SetEnvironmentVariable("MinVerTagPrefix", "v.", EnvironmentVariableTarget.Process);

                Tag(testProject, "v.1.2.3+foo");

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-version-tag");

                // act
                await CleanAndPack(testProject, output, "normal");

                // assert
                AssertPackageFileNameContains("1.2.3.nupkg", output);
            });

        Target(
            "test-package-commit-after-tag",
            DependsOn("test-package-version-tag"),
            async () =>
            {
                // arrange
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
                Environment.SetEnvironmentVariable("MinVerAutoIncrement", "minor", EnvironmentVariableTarget.Process);

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-non-default-auto-increment");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertPackageFileNameContains("1.3.0-alpha.0.1.nupkg", output);
            });

        Target(
            "test-package-annotated-tag",
            DependsOn("test-package-non-default-auto-increment"),
            async () =>
            {
                // arrange
                AnnotatedTag(testProject, "v.1.4.0", "foo");

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
                Environment.SetEnvironmentVariable("MinVerMinimumMajorMinor", "2.0", EnvironmentVariableTarget.Process);

                Commit(testProject);
                Tag(testProject, "v.1.5.0");

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-minimum-major-minor-on-tag");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

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
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertPackageFileNameContains("2.0.0-alpha.0.1.nupkg", output);
            });

        Target(
            "test-package-default-pre-release-phase",
            DependsOn("test-package-minimum-major-minor-after-tag"),
            async () =>
            {
                // arrange
                Environment.SetEnvironmentVariable("MinVerDefaultPreReleasePhase", "preview", EnvironmentVariableTarget.Process);

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-default-pre-release-phase");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertPackageFileNameContains("2.0.0-preview.0.1.nupkg", output);
            });

        Target(
            "test-package-version-override",
            DependsOn("test-package-default-pre-release-phase"),
            async () =>
            {
                // arrange
                Environment.SetEnvironmentVariable("MinVerVersionOverride", "3.2.1-rc.4+build.5", EnvironmentVariableTarget.Process);

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-version-override");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertPackageFileNameContains("3.2.1-rc.4.nupkg", output);
            });

        Target("test-package", DependsOn("test-package-version-override"));

        await RunTargetsAndExitAsync(args);
    }

    private static async Task CleanAndPack(string path, string output, string verbosity)
    {
        Environment.SetEnvironmentVariable("MinVerBuildMetadata", $"build.{buildNumber++}", EnvironmentVariableTarget.Process);

        EnsureEmptyDirectory(output);

        var previousVerbosity = Environment.GetEnvironmentVariable("MinVerVerbosity", EnvironmentVariableTarget.Process);

        Environment.SetEnvironmentVariable("MinVerVerbosity", verbosity ?? "", EnvironmentVariableTarget.Process);
        try
        {
            await RunAsync("dotnet", "build --no-restore --nologo", path);
            await RunAsync("dotnet", $"pack --no-build --output {output} --nologo", path);
        }
        finally
        {
            Environment.SetEnvironmentVariable("MinVerVerbosity", previousVerbosity, EnvironmentVariableTarget.Process);
        }
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
