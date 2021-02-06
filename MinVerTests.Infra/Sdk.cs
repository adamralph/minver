#if !NETCOREAPP2_1
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Builders;

namespace MinVerTests.Infra
{
    public static class Sdk
    {
        private static readonly string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        public static string Version { get; } = Environment.GetEnvironmentVariable("MINVER_TESTS_SDK");

        public static async Task CreateProject(string path, string configuration = Configuration.Current)
        {
            FileSystem.EnsureEmptyDirectory(path);

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

            var source = Solution.GetFullPath($"MinVer/bin/{configuration}/");

            var minVerPackageVersion = Path.GetFileNameWithoutExtension(Directory.EnumerateFiles(source, "*.nupkg").First()).Split("MinVer.", 2)[1];

            _ = await Cli.Wrap("dotnet").WithArguments($"new classlib --name test --output {path}")
                .WithEnvironmentVariables(builder => builder.SetSdk().Build())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync();

            _ = await Cli.Wrap("dotnet").WithArguments($"add package MinVer --source {source} --version {minVerPackageVersion} --package-directory packages")
                .WithEnvironmentVariables(builder => builder.SetSdk().Build())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync();

            _ = await Cli.Wrap("dotnet").WithArguments($"restore --source {source} --packages packages")
                .WithEnvironmentVariables(builder => builder.SetSdk().Build())
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync();
        }

        public static async Task<(Package, string)> BuildProject(string path, params (string, string)[] envVars)
        {
            var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
            _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "diagnostic");
            _ = environmentVariables.TryAdd("GeneratePackageOnBuild", "true");
            _ = environmentVariables.TryAdd("NoPackageAnalysis", "true");

            var result = await Cli.Wrap("dotnet")
                .WithArguments($"build --no-restore{((Version?.StartsWith("2.") ?? false) ? "" : " --nologo")}")
                .WithEnvironmentVariables(builder =>
                {
                    foreach (var pair in environmentVariables)
                    {
                        _ = builder.Set(pair.Key, pair.Value);
                    }

                    _ = builder.SetSdk().Build();
                })
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync();

            var packageFileName = Directory.EnumerateFiles(path, "*.nupkg", new EnumerationOptions { RecurseSubdirectories = true }).First();
            var extractedPackageDirectoryName = Path.Combine(Path.GetDirectoryName(packageFileName), Path.GetFileNameWithoutExtension(packageFileName));
            ZipFile.ExtractToDirectory(packageFileName, extractedPackageDirectoryName);

            var nuspec = await File.ReadAllTextAsync(Directory.EnumerateFiles(extractedPackageDirectoryName, "*.nuspec").First());
            var nuspecVersion = nuspec.Split("<version>")[1].Split("</version>")[0];

            var assemblyFileName = Directory.EnumerateFiles(extractedPackageDirectoryName, "*.dll", new EnumerationOptions { RecurseSubdirectories = true }).First();

            var systemAssemblyVersion = GetAssemblyVersion(assemblyFileName);
            var assemblyVersion = new AssemblyVersion(systemAssemblyVersion.Major, systemAssemblyVersion.Minor, systemAssemblyVersion.Build, systemAssemblyVersion.Revision);

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyFileName);
            var fileVersion = new FileVersion(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart, fileVersionInfo.ProductVersion);

            return (new Package(nuspecVersion, assemblyVersion, fileVersion), result.StandardOutput);
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
