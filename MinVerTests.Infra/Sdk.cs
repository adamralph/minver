#if NET
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;

namespace MinVerTests.Infra
{
    public static class Sdk
    {
        private static readonly string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? "";

        public static string Version { get; } = Environment.GetEnvironmentVariable("MINVER_TESTS_SDK") ?? "";

        public static async Task CreateSolution(string path, string[] projectNames, string configuration = Configuration.Current)
        {
            projectNames = projectNames ?? throw new ArgumentNullException(nameof(projectNames));

            FileSystem.EnsureEmptyDirectory(path);

            CreateGlobalJsonIfRequired(path);

            _ = await DotNet($"new sln --name test --output {path}", path).ConfigureAwait(false);

            var previousProjectName = "";
            foreach (var projectName in projectNames)
            {
                var projectPath = Path.Combine(path, projectName);

                FileSystem.EnsureEmptyDirectory(projectPath);

                await CreateProject(projectPath, configuration, projectName).ConfigureAwait(false);

                // ensure deterministic build order
                if (!string.IsNullOrEmpty(previousProjectName))
                {
                    var projectFileName = Path.Combine(path, projectName, $"{projectName}.csproj");
                    var previousProjectFileName = Path.Combine(path, previousProjectName, $"{previousProjectName}.csproj");

                    _ = await DotNet($"add {projectFileName} reference {previousProjectFileName}", path).ConfigureAwait(false);
                }

                _ = await DotNet($"sln add {projectName}", path).ConfigureAwait(false);

                previousProjectName = projectName;
            }
        }

        public static async Task CreateProject(string path, string configuration = Configuration.Current, bool multiTarget = false)
        {
            FileSystem.EnsureEmptyDirectory(path);

            CreateGlobalJsonIfRequired(path);

            await CreateProject(path, configuration, "test", multiTarget).ConfigureAwait(false);
        }

        private static async Task CreateProject(string path, string configuration, string name, bool multiTarget = false)
        {
            _ = await DotNet($"new classlib --name {name} --output {path}{(multiTarget ? " --langVersion 8.0" : "")}", path).ConfigureAwait(false);

            var source = Solution.GetFullPath($"MinVer/bin/{configuration}/");
            var minVerPackageVersion = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

            _ = await DotNet($"add package MinVer --source {source} --version {minVerPackageVersion} --package-directory packages", path).ConfigureAwait(false);

            if (multiTarget)
            {
                var project = Path.Combine(path, $"{name}.csproj");
                var lines = await File.ReadAllLinesAsync(project).ConfigureAwait(false);

                var editedLines = lines
                    .Select(line => line.Contains("<TargetFramework>", StringComparison.OrdinalIgnoreCase)
                        ? line
                            .Replace("TargetFramework", "TargetFrameworks", StringComparison.OrdinalIgnoreCase)
                            .Replace("</TargetFrameworks>", ";netstandard2.1</TargetFrameworks>", StringComparison.Ordinal)
                        : line);

                await File.WriteAllLinesAsync(project, editedLines).ConfigureAwait(false);
            }

            _ = await DotNet($"restore --source {source} --packages packages", path).ConfigureAwait(false);
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

        public static async Task<(Package Package, string StandardOutput, string StandardError)> BuildProject(string path, params (string, string)[] envVars)
        {
            var (packages, standardOutput, standardError) = await Build(path, envVars).ConfigureAwait(false);

            return (packages.Single(), standardOutput, standardError);
        }

        public static async Task<(List<Package>, string StandardOutput, string StandardError)> Build(string path, params (string, string)[] envVars)
        {
            var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
            _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "diagnostic");
            _ = environmentVariables.TryAdd("GeneratePackageOnBuild", "true");
            _ = environmentVariables.TryAdd("NoPackageAnalysis", "true");

            var (standardOutput, standardError) = await DotNet(
                $"build --no-restore{(!Version.StartsWith("2.", StringComparison.Ordinal) ? " --nologo" : "")}",
                path,
                environmentVariables).ConfigureAwait(false);

            var matcher = new Matcher().AddInclude("**/bin/Debug/*.nupkg");
            var packageFileNames = matcher.GetResultsInFullPath(path).OrderBy(_ => _);
            var getPackages = packageFileNames.Select(async fileName => await GetPackage(fileName).ConfigureAwait(false));
            var packages = await Task.WhenAll(getPackages).ConfigureAwait(false);

            return (packages.ToList(), standardOutput, standardError);
        }

        public static async Task<(string StandardOutput, string StandardError)> Pack(string path, params (string, string)[] envVars)
        {
            var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
            _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "diagnostic");
            _ = environmentVariables.TryAdd("NoPackageAnalysis", "true");

            return await DotNet(
                $"pack --no-restore{(!Version.StartsWith("2.", StringComparison.Ordinal) ? " --nologo" : "")}",
                path,
                environmentVariables).ConfigureAwait(false);
        }

        public static Task<(string StandardOutput, string StandardError)> DotNet(string args, string path, IDictionary<string, string>? envVars = null)
        {
            envVars ??= new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(Version) && !string.IsNullOrWhiteSpace(dotnetRoot))
            {
                envVars["MSBuildExtensionsPath"] = Path.Combine(dotnetRoot, "sdk", Version, "") + Path.DirectorySeparatorChar;
                envVars["MSBuildSDKsPath"] = Path.Combine(dotnetRoot, "sdk", Version, "Sdks");
            }

            return CommandEx.ReadLoggedAsync("dotnet", args, path, envVars);
        }

        private static async Task<Package> GetPackage(string fileName)
        {
            var extractedDirectoryName = Path.Combine(Path.GetDirectoryName(fileName) ?? "", Path.GetFileNameWithoutExtension(fileName));

            ZipFile.ExtractToDirectory(fileName, extractedDirectoryName);

            var nuspecFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.nuspec").First();

            var nuspec = await File.ReadAllTextAsync(nuspecFileName).ConfigureAwait(false);
            var nuspecVersion = nuspec.Split("<version>")[1].Split("</version>")[0];

            var assemblyFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.dll", new EnumerationOptions { RecurseSubdirectories = true }).First();

            var systemAssemblyVersion = GetAssemblyVersion(assemblyFileName);
            var assemblyVersion = new AssemblyVersion(systemAssemblyVersion.Major, systemAssemblyVersion.Minor, systemAssemblyVersion.Build, systemAssemblyVersion.Revision);

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyFileName);
            var fileVersion = new FileVersion(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart, fileVersionInfo.ProductVersion ?? "");

            return new Package(nuspecVersion, assemblyVersion, fileVersion);
        }

        private static Version GetAssemblyVersion(string assemblyFileName)
        {
            var assemblyLoadContext = new AssemblyLoadContext(default, true);
            var assembly = assemblyLoadContext.LoadFromAssemblyPath(assemblyFileName);

            try
            {
                return assembly.GetName().Version ?? throw new InvalidOperationException("The assembly version is null.");
            }
            finally
            {
                assemblyLoadContext.Unload();
            }
        }
    }
}
#endif
