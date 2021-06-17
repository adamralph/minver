#if NET
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Builders;
using Microsoft.Extensions.FileSystemGlobbing;

namespace MinVerTests.Infra
{
    public static class Sdk
    {
        private static readonly string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        public static string Version { get; } = Environment.GetEnvironmentVariable("MINVER_TESTS_SDK");

        public static async Task CreateSolution(string path, string[] projectNames, string configuration = Configuration.Current)
        {
            projectNames ??= Array.Empty<string>();

            FileSystem.EnsureEmptyDirectory(path);

            CreateGlobalJsonIfRequired(path);

            _ = await Cli.Wrap("dotnet").WithArguments($"new sln --name test --output {path}")
                .WithEnvironmentVariables(builder => builder.SetSdk())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync().ConfigureAwait(false);

            string previousProjectName = null;
            foreach (var projectName in projectNames)
            {
                var projectPath = Path.Combine(path, projectName);

                FileSystem.EnsureEmptyDirectory(projectPath);

                await CreateProject(projectPath, configuration, projectName).ConfigureAwait(false);

                // ensure deterministic build order
                if (previousProjectName != null)
                {
                    var projectFileName = Path.Combine(path, projectName, $"{projectName}.csproj");
                    var previousProjectFileName = Path.Combine(path, previousProjectName, $"{previousProjectName}.csproj");

                    _ = await Cli.Wrap("dotnet").WithArguments($"add {projectFileName} reference {previousProjectFileName}")
                        .WithEnvironmentVariables(builder => builder.SetSdk())
                        .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync().ConfigureAwait(false);
                }

                _ = await Cli.Wrap("dotnet").WithArguments($"sln add {projectName}")
                    .WithEnvironmentVariables(builder => builder.SetSdk())
                    .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync().ConfigureAwait(false);

                previousProjectName = projectName;
            }
        }

        public static async Task CreateProject(string path, string configuration = Configuration.Current)
        {
            FileSystem.EnsureEmptyDirectory(path);

            CreateGlobalJsonIfRequired(path);

            await CreateProject(path, configuration, "test").ConfigureAwait(false);
        }

        private static async Task CreateProject(string path, string configuration, string name)
        {
            var source = Solution.GetFullPath($"MinVer/bin/{configuration}/");

            var minVerPackageVersion = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

            _ = await Cli.Wrap("dotnet").WithArguments($"new classlib --name {name} --output {path}")
                .WithEnvironmentVariables(builder => builder.SetSdk())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync().ConfigureAwait(false);

            _ = await Cli.Wrap("dotnet").WithArguments($"add package MinVer --source {source} --version {minVerPackageVersion} --package-directory packages")
                .WithEnvironmentVariables(builder => builder.SetSdk())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync().ConfigureAwait(false);

            _ = await Cli.Wrap("dotnet").WithArguments($"restore --source {source} --packages packages")
                .WithEnvironmentVariables(builder => builder.SetSdk())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync().ConfigureAwait(false);
        }

        private static void CreateGlobalJsonIfRequired(string path)
        {
            if (!string.IsNullOrWhiteSpace(Version))
            {
                File.WriteAllText(
                    Path.Combine(path, "global.json"),
$@"{{
{"  "}""sdk"": {{
{"    "}""version"": ""{Version.Trim()}"",
{"    "}""rollForward"": ""disable""
{"  "}}}
}}
");
            }
        }

        public static async Task<(Package, string)> BuildProject(string path, params (string, string)[] envVars)
        {
            var (packages, @out) = await Build(path, envVars).ConfigureAwait(false);

            return (packages.Single(), @out);
        }

        public static async Task<(List<Package>, string)> Build(string path, params (string, string)[] envVars)
        {
            var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
            _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "diagnostic");
            _ = environmentVariables.TryAdd("GeneratePackageOnBuild", "true");
            _ = environmentVariables.TryAdd("NoPackageAnalysis", "true");

            var result = await Cli.Wrap("dotnet")
                .WithArguments(args => args
                    .Add("build")
                    .Add("--no-restore")
                    .AddIf(!(Version?.StartsWith("2.", StringComparison.Ordinal) ?? false), "--nologo")
                )
                .WithEnvironmentVariables(env => env
                    .SetFrom(environmentVariables)
                    .SetSdk()
                )
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync().ConfigureAwait(false);

            var matcher = new Matcher().AddInclude("**/bin/Debug/*.nupkg");
            var packageFileNames = matcher.GetResultsInFullPath(path).OrderBy(_ => _);
            var getPackages = packageFileNames.Select(async fileName => await GetPackage(fileName).ConfigureAwait(false));
            var packages = await Task.WhenAll(getPackages).ConfigureAwait(false);

            return (packages.ToList(), result.StandardOutput);
        }

        private static async Task<Package> GetPackage(string fileName)
        {
            var extractedDirectoryName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
            ZipFile.ExtractToDirectory(fileName, extractedDirectoryName);

            var nuspec = await File.ReadAllTextAsync(Directory.EnumerateFiles(extractedDirectoryName, "*.nuspec").First()).ConfigureAwait(false);
            var nuspecVersion = nuspec.Split("<version>")[1].Split("</version>")[0];

            var assemblyFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.dll", new EnumerationOptions { RecurseSubdirectories = true }).First();

            var systemAssemblyVersion = GetAssemblyVersion(assemblyFileName);
            var assemblyVersion = new AssemblyVersion(systemAssemblyVersion.Major, systemAssemblyVersion.Minor, systemAssemblyVersion.Build, systemAssemblyVersion.Revision);

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyFileName);
            var fileVersion = new FileVersion(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart, fileVersionInfo.ProductVersion);

            return new Package(nuspecVersion, assemblyVersion, fileVersion);
        }

        private static EnvironmentVariablesBuilder SetSdk(this EnvironmentVariablesBuilder builder) =>
            string.IsNullOrWhiteSpace(Version) || string.IsNullOrWhiteSpace(dotnetRoot)
            ? builder
            : builder
                .Set("MSBuildExtensionsPath", Path.Combine(dotnetRoot, "sdk", Version, "") + Path.DirectorySeparatorChar)
                .Set("MSBuildSDKsPath", Path.Combine(dotnetRoot, "sdk", Version, "Sdks"));

        private static Version GetAssemblyVersion(string assemblyFileName)
        {
            var assemblyLoadContext = new AssemblyLoadContext(default, true);
            var assembly = assemblyLoadContext.LoadFromAssemblyPath(assemblyFileName);

            try
            {
                return assembly.GetName().Version;
            }
            finally
            {
                assemblyLoadContext.Unload();
            }
        }
    }
}
#endif
