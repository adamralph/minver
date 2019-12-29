namespace MinVer.Lib
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading.Tasks;

    internal static class GitCommand
    {
        public static bool TryRun(string args, string workingDirectory, ILogger log, out string output)
        {
            using (var process = new Process())
            {
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
                process.Exited += (s, e) => tcs.SetResult(default);
                process.EnableRaisingEvents = true;

                log.Trace($"Running Git: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                try
                {
                    process.Start();
                }
                catch (Win32Exception ex)
                {
                    throw new Exception("\"git\" is not present in PATH.", ex);
                }

                var runProcess = tcs.Task;
                var readOutput = process.StandardOutput.ReadToEndAsync();
                var readError = process.StandardError.ReadToEndAsync();

                Task.WaitAll(runProcess, readOutput, readError);

                var exitCode = process.ExitCode;
                output = readOutput.Result;
                var error = readError.Result;

                log.Trace($"Git exit code: {exitCode}");
                log.Trace($"Git stdout:{Environment.NewLine}{output}");
                log.Trace($"Git stderr:{Environment.NewLine}{error}");

                return exitCode == 0;
            }
        }
    }
}
