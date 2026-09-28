using MinVer.Lib;

namespace MinVer;

internal sealed class Logger(Verbosity verbosity) : ILogger
{
    public bool IsTraceEnabled => verbosity >= Verbosity.Diagnostic;

    public bool IsDebugEnabled => verbosity >= Verbosity.Detailed;

    public bool IsInfoEnabled => verbosity >= Verbosity.Normal;

    // warnings are deliberately shown at quiet level
    public bool IsWarnEnabled => verbosity >= Verbosity.Quiet;

    public bool Trace(string message) => IsTraceEnabled && Message(message);

    public bool Debug(string message) => IsDebugEnabled && Message(message);

    public bool Info(string message) => IsInfoEnabled && Message(message);

    public bool Warn(int code, string message) => IsWarnEnabled && Message($"warning MINVER{code:D4} : {message}");

    public static void ErrorWorkDirDoesNotExist(string workDir) =>
        Error(1002, $"Working directory '{workDir}' does not exist.");

    public static void ErrorInvalidAutoIncrement(string autoIncrement) =>
        Error(1006, $"Invalid auto increment '{autoIncrement}'. Valid values are {VersionPartExtensions.ValidValues}");

    public static void ErrorInvalidMinMajorMinor(string minMajorMinor) =>
        Error(1003, $"Invalid minimum MAJOR.MINOR '{minMajorMinor}'. Valid values are {MajorMinor.ValidValues}");

    public static void ErrorInvalidVerbosity(string verbosity) =>
        Error(1004, $"Invalid verbosity '{verbosity}'. Valid values are {VerbosityMap.ValidValues}.");

#if MINVER
    public static void ErrorInvalidVersionOverride(string versionOverride) =>
        Error(1005, $"Invalid version override '{versionOverride}'");
#endif

    public static void ErrorNoGit(string message) =>
        Error(1007, message);

    public static void ErrorDefaultPreReleasePhase() =>
        Error(1008, "MinVerDefaultPreReleasePhase is no longer available. Use MinVerDefaultPreReleaseIdentifiers instead, with an additional \"0\" identifier. For example, if you are setting MinVerDefaultPreReleasePhase to \"preview\", set MinVerDefaultPreReleaseIdentifiers to \"preview.0\" instead. In a future version of MinVer, this error message will be removed and MinVerDefaultPreReleasePhase will be ignored.");

    private static void Error(int code, string message) => Message($"error MINVER{code:D4} : {message}");

    private static bool Message(string message)
    {
        if (message.Contains('\r', StringComparison.OrdinalIgnoreCase) || message.Contains('\n', StringComparison.OrdinalIgnoreCase))
        {
            var lines = message.Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase).Split('\r', '\n');
            message = string.Join($"{Environment.NewLine}MinVer: ", lines);
        }

        Console.Error.WriteLine($"MinVer: {message}");

        return true;
    }
}
