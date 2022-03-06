using System;
using System.Linq;
using System.Threading.Tasks;

namespace MinVerTests.Infra
{
    public static class MinVerCli
    {
        public static async Task<(string StandardOutput, string StandardError)> ReadAsync(string workingDirectory, string configuration = Configuration.Current, string args = "", Func<int, bool>? handleExitCode = null, params (string, string)[] envVars)
        {
            var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
            _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "trace");

            return await CommandEx.ReadLoggedAsync("dotnet", $"exec {GetPath(configuration)} {args}", workingDirectory, environmentVariables, handleExitCode).ConfigureAwait(false);
        }

        public static string GetPath(string configuration) =>
#if NETCOREAPP3_1
            Solution.GetFullPath($"minver-cli/bin/{configuration}/netcoreapp3.1/minver-cli.dll");
#endif
#if NET6_0
            Solution.GetFullPath($"minver-cli/bin/{configuration}/net6.0/minver-cli.dll");
#endif
#if NET7_0
            Solution.GetFullPath($"minver-cli/bin/{configuration}/net7.0/minver-cli.dll");
#endif
    }
}
