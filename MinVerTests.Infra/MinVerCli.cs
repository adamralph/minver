using System;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;

namespace MinVerTests.Infra
{
    public static class MinVerCli
    {
        public static async Task<(string, string)> Run(string workingDirectory, string configuration = Configuration.Current, Action<string> log = null, params (string, string)[] envVars)
        {
            var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
            _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "trace");

            var result = await Cli.Wrap("dotnet").WithArguments($"exec {GetPath(configuration)}")
                .WithEnvironmentVariables(environmentVariables)
                .WithWorkingDirectory(workingDirectory).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);

            return (result.StandardOutput.Trim(), result.StandardError);
        }

        public static string GetPath(string configuration) =>
            Solution.GetFullPath($"minver-cli/bin/{configuration}/netcoreapp2.1/minver-cli.dll");
    }
}
