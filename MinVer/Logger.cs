namespace MinVer
{
    using System;
    using MinVer.Lib;
    using Version = MinVer.Lib.Version;

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

        public void WarnIsNotAValidRepositoryOrWorkDirUsingDefaultVersion(string repoOrWorkDir, Version version) =>
            this.Warn(1001, $"'{repoOrWorkDir}' is not a valid repository or working directory. Using default version: {version}.");

        public static void ErrorRepoOrWorkDirDoesNotExist(string repoOrWorkDir) =>
            Error(1002, $"Invalid repository path '{repoOrWorkDir}'. Directory does not exist.");

        public static void ErrorInvalidMinMajorMinor(string minMajorMinor) =>
            Error(1003, $"Invalid minimum MAJOR.MINOR range '{minMajorMinor}'.");

        public static void ErrorInvalidVerbosity(string verbosity) =>
            Error(1004, $"Invalid verbosity level '{verbosity}'. The level must be {VerbosityMap.ToString()}.");

        private void Warn(int code, string message)
        {
            // conditional isn't really required but it gets rid of a compiler warning
            if (this.verbosity >= Verbosity.Quiet)
            {
                Message($"warning MINVER{code:D4} : {message}");
            }
        }

        private static void Error(int code, string message) => Message($"error MINVER{code:D4} : {message}");

        private static void Message(string message) => Console.Error.WriteLine($"MinVer: {message}");
    }
}
