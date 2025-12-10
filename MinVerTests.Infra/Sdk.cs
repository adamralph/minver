using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.FileSystemGlobbing;

namespace MinVerTests.Infra;

public static class Sdk
{
    private static readonly string DotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? "";
    private static readonly string RequiredVersion = Environment.GetEnvironmentVariable("MINVER_TESTS_SDK") ?? "";

    private static readonly Lazy<Task<string>> VersionInUse = new(async () =>
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var (standardOutput, _) = await DotNet("--version", path).ConfigureAwait(false);
        return standardOutput.Trim();
    });

    public static async Task CreateSolution(string path, string[] projectNames, string configuration = Configuration.Current)
    {
        projectNames = projectNames ?? throw new ArgumentNullException(nameof(projectNames));

        FileSystem.EnsureEmptyDirectory(path);

        var minVerPackageSource = GetMinVerPackageSource(configuration);
        var minVerPackageVersion = GetMinVerPackageVersion(minVerPackageSource);

        await CreateGlobalJson(path).ConfigureAwait(false);
        await CreateNugetConfig(path, minVerPackageSource).ConfigureAwait(false);

        _ = await DotNet($"new sln --name test --output {path}", path).ConfigureAwait(false);

        var previousProjectName = "";
        foreach (var projectName in projectNames)
        {
            var projectPath = Path.Combine(path, projectName);

            FileSystem.EnsureEmptyDirectory(projectPath);

            await CreateProject(projectPath, projectName, minVerPackageVersion).ConfigureAwait(false);

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

        var minVerPackageSource = GetMinVerPackageSource(configuration);
        var minVerPackageVersion = GetMinVerPackageVersion(minVerPackageSource);

        await CreateGlobalJson(path).ConfigureAwait(false);
        await CreateNugetConfig(path, minVerPackageSource).ConfigureAwait(false);
        await CreateProject(path, "test", minVerPackageVersion, multiTarget).ConfigureAwait(false);
    }

    private static string GetMinVerPackageSource(string configuration) =>
        Solution.GetFullPath($"MinVer/bin/{configuration}/");

    private static string GetMinVerPackageVersion(string source) =>
        Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

    private static async Task CreateProject(string path, string name, string minVerPackageVersion, bool multiTarget = false)
    {
        _ = await DotNet($"new classlib --name {name} --output {path}{(multiTarget ? " --langVersion 8.0" : "")}", path).ConfigureAwait(false);

        _ = await DotNet($"add package MinVer --version {minVerPackageVersion} --package-directory packages", path).ConfigureAwait(false);

        var project = Path.Combine(path, $"{name}.csproj");
        var lines = await File.ReadAllLinesAsync(project).ConfigureAwait(false);
        var editedLines = lines.Select(line =>
            multiTarget && line.Contains("<TargetFramework>", StringComparison.OrdinalIgnoreCase)
                ? line
                    .Replace("TargetFramework", "TargetFrameworks", StringComparison.OrdinalIgnoreCase)
                    .Replace("</TargetFrameworks>", ";netstandard2.1</TargetFrameworks>", StringComparison.Ordinal)
                : line);

        await File.WriteAllLinesAsync(project, editedLines).ConfigureAwait(false);

        _ = await DotNet("restore --packages packages", path).ConfigureAwait(false);
    }

    private static async Task CreateGlobalJson(string path)
    {
        var version = !string.IsNullOrWhiteSpace(RequiredVersion) ? RequiredVersion : await VersionInUse.Value.ConfigureAwait(false);

        var text =
$$"""
{
{{"  "}}"sdk": {
{{"    "}}"version": "{{version.Trim()}}",
{{"    "}}"rollForward": "disable"
{{"  "}}}
}
""";

        await File.WriteAllTextAsync(Path.Combine(path, "global.json"), text).ConfigureAwait(false);
    }

    private static async Task CreateNugetConfig(string path, string packageSource)
    {
        var lines = new List<string>
        {
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<configuration>",
            "  <packageSources>",
            $"    <add key=\"local\" value=\"{packageSource}\" />",
            "  </packageSources>"
        };

        var clearElements = new List<string> {
            "auditSources",
            "disabledPackageSources",
            "packageSourceMapping"
        };

        lines.AddRange(clearElements.Select(element => $"  <{element}><clear /></{element}>"));
        lines.Add("</configuration>");

        await File.WriteAllLinesAsync(Path.Combine(path, "NuGet.Config"), lines).ConfigureAwait(false);
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
        _ = environmentVariables.TryAdd("IncludeSourceRevisionInInformationalVersion", "false");
        _ = environmentVariables.TryAdd("NoPackageAnalysis", "true");

        // -maxCpuCount:1 is required to prevent massive execution times in GitHub Actions
        var (standardOutput, standardError) = await DotNet(
            "build -maxCpuCount:1 --no-restore --nologo",
            path,
            environmentVariables,
            handleExitCode).ConfigureAwait(false);

        var matcher = new Matcher().AddInclude("**/bin/Debug/*.nupkg");
        var packageFileNames = matcher.GetResultsInFullPath(path).OrderBy(result => result);
        var getPackages = packageFileNames.Select(GetPackage);
        var packages = await Task.WhenAll(getPackages).ConfigureAwait(false);

        return (packages.ToList(), standardOutput, standardError);
    }

    public static Task<(string StandardOutput, string StandardError)> Pack(string path, params (string, string)[] envVars)
    {
        var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
        _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "diagnostic");
        _ = environmentVariables.TryAdd("IncludeSourceRevisionInInformationalVersion", "false");
        _ = environmentVariables.TryAdd("NoPackageAnalysis", "true");

        // -maxCpuCount:1 is required to prevent massive execution times in GitHub Actions
        return DotNet("pack --configuration Debug -maxCpuCount:1 --no-restore --nologo", path, environmentVariables);
    }

    public static Task<(string StandardOutput, string StandardError)> DotNet(string args, string path, IDictionary<string, string>? envVars = null, Func<int, bool>? handleExitCode = null)
    {
        envVars ??= new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(RequiredVersion) && !string.IsNullOrWhiteSpace(DotnetRoot))
        {
            envVars["MSBuildExtensionsPath"] = Path.Combine(DotnetRoot, "sdk", RequiredVersion, "") + Path.DirectorySeparatorChar;
            envVars["MSBuildSDKsPath"] = Path.Combine(DotnetRoot, "sdk", RequiredVersion, "Sdks");
        }

        return CommandEx.ReadLoggedAsync("dotnet", args, path, envVars, handleExitCode);
    }

    private static async Task<Package> GetPackage(string fileName)
    {
        var extractedDirectoryName = Path.Combine(Path.GetDirectoryName(fileName) ?? "", Path.GetFileNameWithoutExtension(fileName));

        ZipFile.ExtractToDirectory(fileName, extractedDirectoryName);

        var nuspecFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.nuspec").First();

        var nuspec = await File.ReadAllTextAsync(nuspecFileName).ConfigureAwait(false);
        var nuspecVersion = nuspec.Split("<version>")[1].Split("</version>")[0];

        var assemblyFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.dll", new EnumerationOptions { RecurseSubdirectories = true }).First();

        var (systemAssemblyVersion, informationalVersion) = GetAssemblyVersions(assemblyFileName);
        var assemblyVersion = new AssemblyVersion(systemAssemblyVersion.Major, systemAssemblyVersion.Minor, systemAssemblyVersion.Build, systemAssemblyVersion.Revision);

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyFileName);
        var fileVersion = new FileVersion(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart, fileVersionInfo.ProductVersion ?? "");

        return new Package(nuspecVersion, assemblyVersion, fileVersion, informationalVersion);
    }

    private static (Version Version, string InformationalVersion) GetAssemblyVersions(string assemblyFileName)
    {
        var assemblyLoadContext = new AssemblyLoadContext(null, true);
        var assembly = assemblyLoadContext.LoadFromAssemblyPath(assemblyFileName);

        try
        {
            var version = assembly.GetName().Version ?? throw new InvalidOperationException("The assembly version is null.");
            var informationalVersion =
                assembly.GetCustomAttributes().OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion ??
                throw new InvalidOperationException("The assembly has no informational version.");

            return (version, informationalVersion);
        }
        finally
        {
            assemblyLoadContext.Unload();
        }
    }
}
