namespace MinVerTests.Infra;

public static class MinVerCli
{
    public static async Task<(string StandardOutput, string StandardError)> ReadAsync(string workingDirectory, string configuration = Configuration.Current, string args = "", Func<int, bool>? handleExitCode = null, params (string, string)[] envVars)
    {
        var environmentVariables = envVars.ToDictionary(envVar => envVar.Item1, envVar => envVar.Item2, StringComparer.OrdinalIgnoreCase);
        _ = environmentVariables.TryAdd("MinVerVerbosity".ToAltCase(), "trace");

        var path = await GetPath(configuration).ConfigureAwait(false);
        return await CommandEx.ReadLoggedAsync("dotnet", $"exec {path} {args}", workingDirectory, environmentVariables, handleExitCode).ConfigureAwait(false);
    }

    public static async Task<string> GetPath(string configuration)
    {
        var targetFramework = await GetTargetFramework().ConfigureAwait(false);
        return Solution.GetFullPath($"minver-cli/bin/{configuration}/{targetFramework}/minver-cli.dll");
    }

    private static async Task<string> GetTargetFramework()
    {
        var sdkVersionInUse = await Sdk.GetVersionInUse().ConfigureAwait(false);
        return sdkVersionInUse.Split('.', 2)[0] == "8" ? "net8.0" : "net9.0";
    }
}
