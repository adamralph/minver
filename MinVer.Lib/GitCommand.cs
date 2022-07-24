using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MinVer.Lib;

internal static class GitCommand
{
    public static bool TryRun(string args, string workingDirectory, ILogger log, out string output)
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

        var tcs = new TaskCompletionSource<object>();
        process.Exited += (_, _) => tcs.SetResult(0);
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

        Task.WaitAll(runProcess, readOutput, readError);

        var exitCode = process.ExitCode;
        output = readOutput.Result;
        var error = readError.Result;

        _ = log.IsTraceEnabled && log.Trace($"Git exit code: {exitCode}");
        _ = log.IsTraceEnabled && log.Trace($"Git stdout:{Environment.NewLine}{output}");
        _ = log.IsTraceEnabled && log.Trace($"Git stderr:{Environment.NewLine}{error}");

        return exitCode == 0;
    }
}
