using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleExec;

namespace MinVerTests.Infra
{
    public static class MinVerCli
    {
        public static async Task<Result> ReadAsync(string workingDirectory, string configuration = Configuration.Current, string args = "", params (string, string)[] envVars)
        {
            var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
            _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "trace");

            return await CommandEx.ReadLoggedAsync("dotnet", $"exec {GetPath(configuration)} {args}", workingDirectory, environmentVariables).ConfigureAwait(false);
        }

        public static string GetPath(string configuration) =>
            Solution.GetFullPath($"minver-cli/bin/{configuration}/netcoreapp3.1/minver-cli.dll");
    }
}
