using System;
using Microsoft.Build.Utilities;
using MinVer.Lib;

namespace MinVer;

internal sealed class Logger(Verbosity verbosity, TaskLoggingHelper taskLoggingHelper) : ILogger
{
    private readonly Verbosity verbosity = verbosity;

    public bool IsTraceEnabled => this.verbosity >= Verbosity.Diagnostic;

    public bool IsDebugEnabled => this.verbosity >= Verbosity.Detailed;

    public bool IsInfoEnabled => this.verbosity >= Verbosity.Normal;

    // warnings are deliberately shown at quiet level
    public bool IsWarnEnabled => this.verbosity >= Verbosity.Quiet;

    public bool Trace(string message) => this.IsTraceEnabled && Message(message);

    public bool Debug(string message) => this.IsDebugEnabled && Message(message);

    public bool Info(string message) => this.IsInfoEnabled && Message(message);

    public bool Warn(int code, string message) => this.IsWarnEnabled && Message($"warning MINVER{code:D4} : {message}");

    public void ErrorWorkDirDoesNotExist(string workDir) =>
        Error(1002, $"Working directory '{workDir}' does not exist.");

    public void ErrorInvalidAutoIncrement(string autoIncrement) =>
        Error(1006, $"Invalid auto increment '{autoIncrement}'. Valid values are {VersionPartExtensions.ValidValues}");

    public void ErrorInvalidMinMajorMinor(string minMajorMinor) =>
        Error(1003, $"Invalid minimum MAJOR.MINOR '{minMajorMinor}'. Valid values are {MajorMinor.ValidValues}");

    public void ErrorInvalidVerbosity(string verbosity) =>
        Error(1004, $"Invalid verbosity '{verbosity}'. Valid values are {VerbosityMap.ValidValues}.");

#if MINVER
    public void ErrorInvalidVersionOverride(string versionOverride) =>
        Error(1005, $"Invalid version override '{versionOverride}'");
#endif

    public void ErrorNoGit(string message) =>
        Error(1007, message);

    private void Error(int code, string message) => Message($"error MINVER{code:D4} : {message}", true);

    private bool Message(string message, bool error = false)
    {
        if (message.Contains('\r', StringComparison.OrdinalIgnoreCase) || message.Contains('\n', StringComparison.OrdinalIgnoreCase))
        {
            var lines = message.Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase).Split('\r', '\n');
            message = string.Join($"{Environment.NewLine}MinVer: ", lines);
        }

        if (error)
        {
            taskLoggingHelper.LogError($"MinVer: {message}");
        }
        else
        {
            taskLoggingHelper.LogMessage($"MinVer: {message}");
        }

        return true;
    }
}
