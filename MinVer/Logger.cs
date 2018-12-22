namespace MinVer
{
    using System;
    using MinVer.Lib;

    internal class Logger : ILogger
    {
        private readonly Verbosity verbosity;

        public Logger(Verbosity verbosity) => this.verbosity = verbosity;

        public bool IsTraceEnabled => this.verbosity >= Verbosity.Diagnostic;

        public bool IsDebugEnabled => this.verbosity >= Verbosity.Detailed;

        public void Trace(string message)
        {
            if (this.verbosity >= Verbosity.Diagnostic)
            {
                Message(message);
            }
        }

        public void Debug(string message)
        {
            if (this.verbosity >= Verbosity.Detailed)
            {
                Message(message);
            }
        }

        public void Info(string message)
        {
            if (this.verbosity >= Verbosity.Normal)
            {
                Message(message);
            }
        }

        public void Warn(int code, string message)
        {
            if (this.verbosity >= Verbosity.Quiet)
            {
                Message($"warning MINVER{code:D4} : {message}");
            }
        }

        public static void ErrorRepoOrWorkDirDoesNotExist(string repoOrWorkDir) =>
            Error(1002, $"Repository or working directory '{repoOrWorkDir}' does not exist.");

        public static void ErrorInvalidMinMajorMinor(string minMajorMinor) =>
            Error(1003, $"Invalid minimum MAJOR.MINOR '{minMajorMinor}'. Valid values are {MajorMinor.ValidValues}");

        public static void ErrorInvalidVerbosity(string verbosity) =>
            Error(1004, $"Invalid verbosity '{verbosity}'. The value must be {VerbosityMap.ValidValue}.");

#if MINVER
        public static void ErrorInvalidVersionOverride(string versionOverride) =>
            Error(1005, $"Invalid version override '{versionOverride}'");
#endif

        private static void Error(int code, string message) => Message($"error MINVER{code:D4} : {message}");

        private static void Message(string message) => Console.Error.WriteLine($"MinVer: {message}");
    }
}
