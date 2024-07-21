namespace MinVerTests.Infra;

public static class MinVerCli
{
    public static Task<(string StandardOutput, string StandardError)> ReadAsync(string workingDirectory, string configuration = Configuration.Current, string args = "", Func<int, bool>? handleExitCode = null, params (string, string)[] envVars)
    {
        var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
        _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "trace");

        return CommandEx.ReadLoggedAsync("dotnet", $"exec {GetPath(configuration)} {args}", workingDirectory, environmentVariables, handleExitCode);
    }

    public static string GetPath(string configuration) =>
#if NET6_0
        Solution.GetFullPath($"minver-cli/bin/{configuration}/net6.0/minver-cli.dll");
#endif
#if NET8_0
        Solution.GetFullPath($"minver-cli/bin/{configuration}/net8.0/minver-cli.dll");
#endif
}
