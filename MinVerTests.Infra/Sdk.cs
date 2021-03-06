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

        public static async Task CreateSolution(string path, string[] projectNames, string configuration = Configuration.Current, Action<string> log = null)
        {
            projectNames ??= Array.Empty<string>();

            FileSystem.EnsureEmptyDirectory(path);

            CreateGlobalJsonIfRequired(path);

            _ = await Cli.Wrap("dotnet").WithArguments($"new sln --name test --output {path}")
                .WithEnvironmentVariables(builder => builder.SetSdk())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);

            string previousProjectName = null;
            foreach (var projectName in projectNames)
            {
                var projectPath = Path.Combine(path, projectName);

                FileSystem.EnsureEmptyDirectory(projectPath);

                await CreateProject(projectPath, configuration, projectName, log).ConfigureAwait(false);

                // ensure deterministic build order
                if (previousProjectName != null)
                {
                    var projectFileName = Path.Combine(path, projectName, $"{projectName}.csproj");
                    var previousProjectFileName = Path.Combine(path, previousProjectName, $"{previousProjectName}.csproj");

                    _ = await Cli.Wrap("dotnet").WithArguments($"add {projectFileName} reference {previousProjectFileName}")
                        .WithEnvironmentVariables(builder => builder.SetSdk())
                        .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);
                }

                _ = await Cli.Wrap("dotnet").WithArguments($"sln add {projectName}")
                    .WithEnvironmentVariables(builder => builder.SetSdk())
                    .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);

                previousProjectName = projectName;
            }
        }

        public static async Task CreateProject(string path, string configuration = Configuration.Current, Action<string> log = null)
        {
            FileSystem.EnsureEmptyDirectory(path);

            CreateGlobalJsonIfRequired(path);

            await CreateProject(path, configuration, "test", log).ConfigureAwait(false);
        }

        private static async Task CreateProject(string path, string configuration, string name, Action<string> log)
        {
            var source = Solution.GetFullPath($"MinVer/bin/{configuration}/");

            var minVerPackageVersion = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

            _ = await Cli.Wrap("dotnet").WithArguments($"new classlib --name {name} --output {path}")
                .WithEnvironmentVariables(builder => builder.SetSdk())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);

            _ = await Cli.Wrap("dotnet").WithArguments($"add package MinVer --source {source} --version {minVerPackageVersion} --package-directory packages")
                .WithEnvironmentVariables(builder => builder.SetSdk())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);

            _ = await Cli.Wrap("dotnet").WithArguments($"restore --source {source} --packages packages")
                .WithEnvironmentVariables(builder => builder.SetSdk())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);
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

        public static async Task<(Package, string)> BuildProject(string path, Action<string> log = null, params (string, string)[] envVars)
        {
            var (packages, @out) = await Build(path, log, envVars).ConfigureAwait(false);

            return (packages.Single(), @out);
        }

        public static async Task<(List<Package>, string)> Build(string path, Action<string> log = null, params (string, string)[] envVars)
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
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);

            log?.Invoke("Read packages...");

            var matcher = new Matcher().AddInclude("**/bin/Debug/*.nupkg");
            var packageFileNames = matcher.GetResultsInFullPath(path).OrderBy(_ => _);
            var getPackages = packageFileNames.Select(async fileName => await GetPackage(fileName, log).ConfigureAwait(false));
            var packages = await Task.WhenAll(getPackages).ConfigureAwait(false);

            return (packages.ToList(), result.StandardOutput);
        }

        private static async Task<Package> GetPackage(string fileName, Action<string> log)
        {
            var extractedDirectoryName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));

            log?.Invoke($"Extracting '{fileName}' to '{extractedDirectoryName}'...");
            ZipFile.ExtractToDirectory(fileName, extractedDirectoryName);
            log?.Invoke($"Finished extracting '{fileName}' to '{extractedDirectoryName}'");

            log?.Invoke($"Finding nuspec...");
            var nuspecFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.nuspec").First();
            log?.Invoke($"Finished finding nuspec");

            log?.Invoke($"Reading '{nuspecFileName}'...");
            var nuspec = await File.ReadAllTextAsync(nuspecFileName).ConfigureAwait(false);
            log?.Invoke($"Finished reading '{nuspecFileName}'");

            var nuspecVersion = nuspec.Split("<version>")[1].Split("</version>")[0];

            log?.Invoke($"Finding assembly...");
            var assemblyFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.dll", new EnumerationOptions { RecurseSubdirectories = true }).First();
            log?.Invoke($"Finished finding assembly");

            log?.Invoke($"Getting assembly version...");
            var systemAssemblyVersion = GetAssemblyVersion(assemblyFileName);
            log?.Invoke($"Finished getting assembly version");

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
