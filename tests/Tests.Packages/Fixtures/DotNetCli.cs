using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using Fixtures;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualStudio.Threading;

namespace Tests.Packages.Fixtures;

public static class DotNetCli
{
    private static readonly string DotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? "";
    private static readonly string RequiredVersion = Environment.GetEnvironmentVariable("MINVER_SDK") ?? "";

#pragma warning disable VSTHRD012 // Provide JoinableTaskFactory where allowed
    private static readonly AsyncLazy<string> VersionInUse = new(async () =>
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var (standardOutput, _) = await Read("--version", path).ConfigureAwait(false);
            return standardOutput.Trim();
        });
#pragma warning restore VSTHRD012

    public static Task<string> GetVersionInUse() => VersionInUse.GetValueAsync();

    public static async Task<string> GetVersionForTestProjects() =>
        !string.IsNullOrWhiteSpace(RequiredVersion) ? RequiredVersion : await VersionInUse.GetValueAsync().ConfigureAwait(false);

    public static async Task CreateSolution(string path, string[] projectNames)
    {
        projectNames = projectNames ?? throw new ArgumentNullException(nameof(projectNames));

        FileSystem.EnsureEmptyDirectory(path);

        var minVerPackageSource = GetMinVerPackageSource();
        var minVerPackageVersion = GetMinVerPackageVersion(minVerPackageSource);

        await CreateGlobalJson(path).ConfigureAwait(false);
        await CreateNugetConfig(path, minVerPackageSource).ConfigureAwait(false);

        _ = await Read($"new sln --name test --output {path}", path).ConfigureAwait(false);

        var previousProjectName = "";
        foreach (var projectName in projectNames)
        {
            var projectPath = Path.Combine(path, projectName);

            FileSystem.EnsureEmptyDirectory(projectPath);

            // as with the default global packages folder, the projects use the same copy of the MinVer package
            await CreateProject(projectPath, projectName, minVerPackageVersion, packagesPath: Path.Combine(path, "packages")).ConfigureAwait(false);

            // ensure deterministic build order
            if (!string.IsNullOrEmpty(previousProjectName))
            {
                var projectFileName = Path.Combine(path, projectName, $"{projectName}.csproj");
                var previousProjectFileName = Path.Combine(path, previousProjectName, $"{previousProjectName}.csproj");

                _ = await Read($"add {projectFileName} reference {previousProjectFileName}", path).ConfigureAwait(false);
            }

            _ = await Read($"sln add {projectName}", path).ConfigureAwait(false);

            previousProjectName = projectName;
        }
    }

    public static async Task CreateProject(string path, bool multiTarget = false)
    {
        FileSystem.EnsureEmptyDirectory(path);

        var minVerPackageSource = GetMinVerPackageSource();
        var minVerPackageVersion = GetMinVerPackageVersion(minVerPackageSource);

        await CreateGlobalJson(path).ConfigureAwait(false);
        await CreateNugetConfig(path, minVerPackageSource).ConfigureAwait(false);
        await CreateProject(path, "test", minVerPackageVersion, multiTarget).ConfigureAwait(false);
    }

    private static string GetMinVerPackageSource() =>
        Solution.GetFullPath("artifacts");

    private static string GetMinVerPackageVersion(string source) =>
        Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "MinVer.*.nupkg").First()).Split("MinVer.", 2)[1];

    private static async Task CreateProject(string path, string name, string minVerPackageVersion, bool multiTarget = false, string packagesPath = "packages")
    {
        _ = await Read($"new classlib --name {name} --output {path}{(multiTarget ? " --langVersion 12.0" : "")}", path).ConfigureAwait(false);

        _ = await Read($"add package MinVer --version {minVerPackageVersion} --package-directory {packagesPath}", path).ConfigureAwait(false);

        var project = Path.Combine(path, $"{name}.csproj");
        var lines = await File.ReadAllLinesAsync(project).ConfigureAwait(false);
        var editedLines = lines.Select(line =>
            multiTarget && line.Contains("<TargetFramework>", StringComparison.OrdinalIgnoreCase)
                ? line
                    .Replace("TargetFramework", "TargetFrameworks", StringComparison.OrdinalIgnoreCase)
                    .Replace("</TargetFrameworks>", ";netstandard2.1</TargetFrameworks>", StringComparison.Ordinal)
                : line);

        await File.WriteAllLinesAsync(project, editedLines).ConfigureAwait(false);

        _ = await Read($"restore --packages {packagesPath}", path).ConfigureAwait(false);
    }

    private static async Task CreateGlobalJson(string path)
    {
        var version = await GetVersionForTestProjects().ConfigureAwait(false);

        var text =
$$"""
{
    "sdk": {
        "version": "{{version.Trim()}}",
        "rollForward": "disable"
    }
}
""";

        await File.WriteAllTextAsync(Path.Combine(path, "global.json"), text).ConfigureAwait(false);
    }

    private static async Task CreateNugetConfig(string path, string packageSource)
    {
        var text =
$"""
<configuration>
    <auditSources><clear /></auditSources>
    <disabledPackageSources><clear /></disabledPackageSources>
    <packageSourceMapping><clear /></packageSourceMapping>
    <packageSources>
        <clear />
        <add key="local" value="{packageSource}" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    </packageSources>
</configuration>
""";

        await File.WriteAllTextAsync(Path.Combine(path, "NuGet.Config"), text).ConfigureAwait(false);
    }

    public static async Task<(Package? Package, string StandardOutput, string StandardError)> BuildProject(string path, Func<int, bool>? handleExitCode = null, params (string, string)[] envVars)
    {
        var (packages, standardOutput, standardError) = await Build(path, handleExitCode, envVars).ConfigureAwait(false);
        return (packages.SingleOrDefault(), standardOutput, standardError);
    }

    public static Task<(List<Package>, string StandardOutput, string StandardError)> Build(string path, Func<int, bool>? handleExitCode = null, params (string, string)[] envVars) =>
        // -maxCpuCount:1 is required to prevent massive execution times in GitHub Actions
        Build("build -maxCpuCount:1 --no-restore", path, handleExitCode, envVars);

    public static Task<(List<Package>, string StandardOutput, string StandardError)> BuildMultiProcess(string path) =>
        Build("build -maxCpuCount:4 -nodeReuse:false --no-restore", path, null, []);

    // only supported by SDK 11 and later
    public static Task<(List<Package>, string StandardOutput, string StandardError)> BuildMultiThreaded(string path) =>
        // the MSBuild server is otherwise started implicitly, and outlives the test
        Build("build -mt -maxCpuCount:4 -warnAsError:MSB4286,MSB4287,MSB4288 --no-restore", path, null, [("MSBUILDUSESERVER", "0"),]);

    private static async Task<(List<Package>, string StandardOutput, string StandardError)> Build(string args, string path, Func<int, bool>? handleExitCode, (string, string)[] envVars)
    {
        var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
        _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "diagnostic");
        _ = environmentVariables.TryAdd("GeneratePackageOnBuild", "true");
        _ = environmentVariables.TryAdd("IncludeSourceRevisionInInformationalVersion", "false");
        _ = environmentVariables.TryAdd("NoPackageAnalysis", "true");

        var (standardOutput, standardError) = await Read(
            args,
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
        return Read("pack --configuration Debug -maxCpuCount:1 --no-restore", path, environmentVariables);
    }

    public static Task<(string StandardOutput, string StandardError)> Clean(string path, params (string, string)[] envVars) =>
        // -maxCpuCount:1 is required to prevent massive execution times in GitHub Actions
        Read($"clean -maxCpuCount:1", path, envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase));

    private static Task<(string StandardOutput, string StandardError)> Read(string args, string path, Dictionary<string, string>? envVars = null, Func<int, bool>? handleExitCode = null)
    {
        envVars ??= [];

        if (!string.IsNullOrWhiteSpace(RequiredVersion) && !string.IsNullOrWhiteSpace(DotnetRoot))
        {
            envVars["MSBuildExtensionsPath"] = Path.Combine(DotnetRoot, "sdk", RequiredVersion, "") + Path.DirectorySeparatorChar;
            envVars["MSBuildSDKsPath"] = Path.Combine(DotnetRoot, "sdk", RequiredVersion, "Sdks");
        }

        return LoggingCommand.ReadAsync("dotnet", args, path, envVars, handleExitCode);
    }

    private static async Task<Package> GetPackage(string fileName)
    {
        var extractedDirectoryName = Path.Combine(Path.GetDirectoryName(fileName) ?? "", Path.GetFileNameWithoutExtension(fileName));

#if NET10_0_OR_GREATER
        await ZipFile.ExtractToDirectoryAsync(fileName, extractedDirectoryName).ConfigureAwait(false);
#else
        ZipFile.ExtractToDirectory(fileName, extractedDirectoryName);
#endif

        var nuspecFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.nuspec").First();

        var nuspec = await File.ReadAllTextAsync(nuspecFileName).ConfigureAwait(false);
        var nuspecVersion = nuspec.Split("<version>")[1].Split("</version>")[0];

        var assemblyFileName = Directory.EnumerateFiles(extractedDirectoryName, "*.dll", new EnumerationOptions { RecurseSubdirectories = true, }).First();

        // read from metadata, since a newer SDK may produce an assembly for a newer runtime than this one
        var systemAssemblyVersion = AssemblyName.GetAssemblyName(assemblyFileName).Version ?? throw new InvalidOperationException("The assembly version is null.");
        var assemblyVersion = new AssemblyVersion(systemAssemblyVersion.Major, systemAssemblyVersion.Minor, systemAssemblyVersion.Build, systemAssemblyVersion.Revision);

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyFileName);
        var informationalVersion = fileVersionInfo.ProductVersion ?? throw new InvalidOperationException("The assembly has no informational version.");
        var fileVersion = new FileVersion(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart, informationalVersion);

        return new Package(nuspecVersion, assemblyVersion, fileVersion, informationalVersion);
    }
}
