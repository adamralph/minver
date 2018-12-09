namespace MinVer
{
    using System;
    using MinVer.Lib;
    using Version = MinVer.Lib.Version;

    internal class Logger : ILogger
    {
        private readonly Verbosity verbosity;

        public Logger(Verbosity verbosity) => this.verbosity = verbosity;

        public bool IsTraceEnabled => this.verbosity >= Verbosity.Trace;

        public bool IsDebugEnabled => this.verbosity >= Verbosity.Debug;

        public void Trace(string message)
        {
            if (this.verbosity >= Verbosity.Trace)
            {
                Message(message);
            }
        }

        public void Debug(string message)
        {
            if (this.verbosity >= Verbosity.Debug)
            {
                Message(message);
            }
        }

        public void Info(string message)
        {
            if (this.verbosity >= Verbosity.Info)
            {
                Message(message);
            }
        }

        public void WarnIsNotAValidRepositoryOrWorkDirUsingDefaultVersion(string repoOrWorkDir, Version defaultVersion) =>
            this.Warn($"'{repoOrWorkDir}' is not a valid repository or working directory. Using default version: {defaultVersion}.");

        public static void ErrorRepoOrWorkDirDoesNotExist(string repoOrWorkDir) =>
            Error($"Invalid repository path '{repoOrWorkDir}'. Directory does not exist.");

        public static void ErrorInvalidMinMajorMinor(string minMajorMinor) =>
            Error($"Invalid minimum MAJOR.MINOR range '{minMajorMinor}'.");

        public static void ErrorInvalidVerbosity(string verbosity) =>
            Error($"Invalid verbosity level '{verbosity}'. The level must be {VerbosityMap.ToString()}.");

        private void Warn(string message)
        {
            if (this.verbosity >= Verbosity.Warn)
            {
                Message($"warning : {message}");
            }
        }

        private static void Error(string message) => Message($"error : {message}");

        private static void Message(string message) => Console.Error.WriteLine($"MinVer: {message}");
    }
}
