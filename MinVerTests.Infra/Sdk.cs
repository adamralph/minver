using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;

namespace MinVerTests.Infra;

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

    public static Task CreateProject(string path, string configuration = Configuration.Current, bool multiTarget = false, bool enableSourceLink = false)
    {
        FileSystem.EnsureEmptyDirectory(path);

        CreateGlobalJsonIfRequired(path);

        return CreateProject(path, configuration, "test", multiTarget, enableSourceLink);
    }

    private static async Task CreateProject(string path, string configuration, string name, bool multiTarget = false, bool enableSourceLink = false)
    {
        _ = await DotNet($"new classlib --name {name} --output {path}{(multiTarget ? " --langVersion 8.0" : "")}", path).ConfigureAwait(false);

        var source = Solution.GetFullPath($"MinVer/bin/{configuration}/");
        var minVerPackageVersion = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

        _ = await DotNet($"add package MinVer --source {source} --version {minVerPackageVersion} --package-directory packages", path).ConfigureAwait(false);

        var project = Path.Combine(path, $"{name}.csproj");
        var lines = await File.ReadAllLinesAsync(project).ConfigureAwait(false);
        var editedLines = new List<string>();

        foreach (var line in lines)
        {
            if (line.Contains("<TargetFramework>", StringComparison.OrdinalIgnoreCase))
            {
                if (multiTarget)
                {
                    editedLines.Add(line
                        .Replace("TargetFramework", "TargetFrameworks", StringComparison.OrdinalIgnoreCase)
                        .Replace("</TargetFrameworks>", ";netstandard2.1</TargetFrameworks>", StringComparison.Ordinal));
                }
                else
                {
                    editedLines.Add(line);
                }

                if (!enableSourceLink)
                {
                    editedLines.Add("<IncludeSourceRevisionInInformationalVersion>false</IncludeSourceRevisionInInformationalVersion>");
                }
            }
            else
            {
                editedLines.Add(line);
            }
        }

        await File.WriteAllLinesAsync(project, editedLines).ConfigureAwait(false);

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

    public static async Task<(Package? Package, string StandardOutput, string StandardError)> BuildProject(string path, Func<int, bool>? handleExitCode = null, params (string, string)[] envVars)
    {
        var (packages, standardOutput, standardError) = await Build(path, handleExitCode, envVars).ConfigureAwait(false);

        return (packages.SingleOrDefault(), standardOutput, standardError);
    }

    public static async Task<(List<Package>, string StandardOutput, string StandardError)> Build(string path, Func<int, bool>? handleExitCode = null, params (string, string)[] envVars)
    {
        var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
        _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "diagnostic");
        _ = environmentVariables.TryAdd("GeneratePackageOnBuild", "true");
        _ = environmentVariables.TryAdd("NoPackageAnalysis", "true");

        // -maxCpuCount:1 is required to prevent massive execution times in GitHub Actions
        var (standardOutput, standardError) = await DotNet(
            $"build -maxCpuCount:1 --no-restore{(!Version.StartsWith("2.", StringComparison.Ordinal) ? " --nologo" : "")}",
            path,
            environmentVariables,
            handleExitCode).ConfigureAwait(false);

        //very weird, it always put the file in bin/x64/Debug and not bin/Debug changed it for now
        var matcher = new Matcher().AddInclude("**/Debug/*.nupkg");
        var packageFileNames = matcher.GetResultsInFullPath(path).OrderBy(result => result);
        var getPackages = packageFileNames.Select(GetPackage);
        var packages = await Task.WhenAll(getPackages).ConfigureAwait(false);

        return (packages.ToList(), standardOutput, standardError);
    }

    public static Task<(string StandardOutput, string StandardError)> Pack(string path, params (string, string)[] envVars)
    {
        var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
        _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "diagnostic");
        _ = environmentVariables.TryAdd("NoPackageAnalysis", "true");

        // -maxCpuCount:1 is required to prevent massive execution times in GitHub Actions
        return DotNet(
            $"pack --configuration Debug -maxCpuCount:1 --no-restore{(!Version.StartsWith("2.", StringComparison.Ordinal) ? " --nologo" : "")}",
            path,
            environmentVariables);
    }

    public static Task<(string StandardOutput, string StandardError)> DotNet(string args, string path, IDictionary<string, string>? envVars = null, Func<int, bool>? handleExitCode = null)
    {
        envVars ??= new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(Version) && !string.IsNullOrWhiteSpace(dotnetRoot))
        {
            envVars["MSBuildExtensionsPath"] = Path.Combine(dotnetRoot, "sdk", Version, "") + Path.DirectorySeparatorChar;
            envVars["MSBuildSDKsPath"] = Path.Combine(dotnetRoot, "sdk", Version, "Sdks");
        }

        return CommandEx.ReadLoggedAsync("dotnet", args, path, envVars, handleExitCode);
    }

    private static async Task<Package> GetPackage(string fileName)
    {
        var extractedDirectoryName = Path.Combine(Path.GetDirectoryName(fileName) ?? "", Path.GetFileNameWithoutExtension(fileName));

        ZipFile.ExtractToDirectory(fileName, extractedDirectoryName);

        var nuspecFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.nuspec").First();

        var nuspec = await File.ReadAllTextAsync(nuspecFileName).ConfigureAwait(false);

        //adding Revision part to value got from nuget because of automatic truncation by nuget (i have a feeling this could be done better)
        var nuspecVersionRaw = nuspec.Split("<version>")[1].Split("</version>")[0];
        var nuspecVersionParts = nuspecVersionRaw.Split('-', 2);
        var containsRevision = nuspecVersionParts[0].Count(c => c == '.') > 2;
        string nuspecVersion;
        if (containsRevision)
        {
            nuspecVersion = nuspecVersionRaw;
        }
        else
        {
            nuspecVersion = $"{nuspecVersionParts[0]}.0";
            nuspecVersion += nuspecVersionParts.Length > 1 ? $"-{nuspecVersionParts[1]}" : string.Empty;
        }

        var assemblyFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.dll", new EnumerationOptions { RecurseSubdirectories = true, }).First();

        var (systemAssemblyVersion, informationalVersion) = GetAssemblyVersions(assemblyFileName);
        //also changing informational version because of automatic truncation
        if (informationalVersion.Count(c => c == '.') < 3)
        {
            informationalVersion += ".0";
        }

        var assemblyVersion = new AssemblyVersion(systemAssemblyVersion.Major, systemAssemblyVersion.Minor, systemAssemblyVersion.Build, systemAssemblyVersion.Revision);

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyFileName);
        //also changing product version because of automatic truncation
        var productVersion = fileVersionInfo.ProductVersion?.Count(c => c == '.') < 3
            ? $"{fileVersionInfo.ProductVersion}.0"
            : fileVersionInfo.ProductVersion;
        var fileVersion = new FileVersion(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart, productVersion ?? "");

        return new Package(nuspecVersion, assemblyVersion, fileVersion, informationalVersion);
    }

    private static (Version Version, string InformationalVersion) GetAssemblyVersions(string assemblyFileName)
    {
        var assemblyLoadContext = new AssemblyLoadContext(default, true);
        var assembly = assemblyLoadContext.LoadFromAssemblyPath(assemblyFileName);

        try
        {
            return (
                assembly.GetName().Version ?? throw new InvalidOperationException("The assembly version is null."),
                assembly.GetCustomAttributes().OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion ??
                throw new InvalidOperationException("The assembly has no informational version."));
        }
        finally
        {
            assemblyLoadContext.Unload();
        }
    }
}
