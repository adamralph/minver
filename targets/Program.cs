using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Xunit;
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

                File.WriteAllText(
                    Path.Combine(testProject, "global.json"),
@"{
  ""sdk"": {
    ""version"": ""2.1.300"",
    ""rollForward"": ""latestMajor""
  }
}
"
                    );

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
                AssertVersion(new Version(0, 0, 0, new[] { "alpha", "0" }), output);
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
                AssertVersion(new Version(0, 0, 0, new[] { "alpha", "0" }), output);
            });

        Target(
            "test-package-commit",
            DependsOn("test-package-no-commits"),
            async () =>
            {
                // arrange
                PrepareForCommits(testProject);
                Commit(testProject);

                var output = Path.Combine(testPackageBaseOutput, $"{buildNumber}-test-package-commit");

                // act
                await CleanAndPack(testProject, output, "diagnostic");

                // assert
                AssertVersion(new Version(0, 0, 0, new[] { "alpha", "0" }), output);
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
                AssertVersion(new Version(0, 0, 0, new[] { "alpha", "0" }), output);
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
                AssertVersion(new Version(1, 2, 3, default, default, "foo"), output);
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
                AssertVersion(new Version(1, 2, 4, new[] { "alpha", "0" }, 1), output);
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
                AssertVersion(new Version(1, 3, 0, new[] { "alpha", "0" }, 1), output);
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
                AssertVersion(new Version(1, 4, 0), output);
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
                AssertVersion(new Version(2, 0, 0, new[] { "alpha", "0" }), output);
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
                AssertVersion(new Version(2, 0, 0, new[] { "alpha", "0" }, 1), output);
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
                AssertVersion(new Version(1, 5, 1, new[] { "preview", "0" }, 1), output);
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
                AssertVersion(new Version(3, 2, 1, new[] { "rc", "4" }, default, "build.5"), output);
            });

        Target("test-package", DependsOn("test-package-version-override"));

        Target("default", DependsOn("test-api", "test-package"));

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

    private static void AssertVersion(Version expected, string path)
    {
        var packagePath = Directory.EnumerateFiles(path, "*.nupkg", new EnumerationOptions { RecurseSubdirectories = true })
            .First();

        Assert.EndsWith(expected.ToString().Split('+')[0], Path.GetFileNameWithoutExtension(packagePath));

        ZipFile.ExtractToDirectory(
            packagePath,
            Path.Combine(Path.GetDirectoryName(packagePath), Path.GetFileNameWithoutExtension(packagePath)));

        var assemblyPath = Directory.EnumerateFiles(path, "*.dll", new EnumerationOptions { RecurseSubdirectories = true })
            .First();

        var context = new AssemblyLoadContext(default, true);
        var assembly = context.LoadFromAssemblyPath(assemblyPath);
        var assemblyVersion = assembly.GetName().Version;
        context.Unload();

        var fileVersion = FileVersionInfo.GetVersionInfo(assemblyPath);

        Assert.Equal(expected.Major, assemblyVersion.Major);
        Assert.Equal(0, assemblyVersion.Minor);
        Assert.Equal(0, assemblyVersion.Build);

        Assert.Equal(expected.Major, fileVersion.FileMajorPart);
        Assert.Equal(expected.Minor, fileVersion.FileMinorPart);
        Assert.Equal(expected.Patch, fileVersion.FileBuildPart);

        Assert.Equal(expected.ToString(), fileVersion.ProductVersion);
    }

    public class Version
    {
        private readonly List<string> preReleaseIdentifiers;
        private readonly int height;
        private readonly string buildMetadata;

        public Version(int major, int minor, int patch, IEnumerable<string> preReleaseIdentifiers = null, int height = 0, string buildMetadata = null)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.preReleaseIdentifiers = preReleaseIdentifiers?.ToList() ?? new List<string>();
            this.height = height;
            this.buildMetadata = buildMetadata;
        }

        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public override string ToString() =>
            $"{this.Major}.{this.Minor}.{this.Patch}{(this.preReleaseIdentifiers.Count == 0 ? "" : $"-{string.Join(".", this.preReleaseIdentifiers)}")}{(this.height == 0 ? "" : $".{this.height}")}{(string.IsNullOrEmpty(this.buildMetadata) ? "" : $"+{this.buildMetadata}")}";
    }
}
