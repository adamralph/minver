using System.ComponentModel;
using System.Diagnostics;

namespace MinVer.Lib;

internal static class GitCommand
{
    public static async Task<string?> TryRun(string args, string workingDirectory, ILogger log)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = args,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        var tcs = new TaskCompletionSource();
        process.Exited += (_, _) => tcs.SetResult();
        process.EnableRaisingEvents = true;

        _ = log.IsTraceEnabled && log.Trace($"Running Git: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

        try
        {
            _ = process.Start();
        }
        catch (Win32Exception ex)
        {
            throw new NoGitException("\"git\" is not present in PATH.", ex);
        }

        var runProcess = tcs.Task;
        var readOutput = process.StandardOutput.ReadToEndAsync();
        var readError = process.StandardError.ReadToEndAsync();

        await runProcess;
        var exitCode = process.ExitCode;
        var output = await readOutput;
        var error = await readError;

        _ = log.IsTraceEnabled && log.Trace($"Git exit code: {exitCode}");
        _ = log.IsTraceEnabled && log.Trace($"Git stdout:{Environment.NewLine}{output}");
        _ = log.IsTraceEnabled && log.Trace($"Git stderr:{Environment.NewLine}{error}");

        return exitCode == 0 ? output : null;
    }
}
