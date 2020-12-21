namespace MinVer
{
    using System;
    using MinVer.Lib;

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

        public void Warn(int code, string message)
        {
            if (this.verbosity >= Verbosity.Warn)
            {
                Message($"warning : {message}");
            }
        }

        public static void Warn(string message) => Message($"warning : {message}");

        public static void ErrorInvalidEnvVar(string name, string value, string validValueString)
        {
            if (validValueString == null)
            {
                Error($"Invalid environment variable '{name}' '{value}'.");
            }
            else
            {
                Error($"Invalid environment variable '{name}' '{value}'. Valid values are {validValueString}");
            }
        }

        public static void ErrorWorkDirDoesNotExist(string workDir) =>
            Error($"Working directory '{workDir}' does not exist.");

        public static void ErrorInvalidAutoIncrement(string autoIncrement) =>
            Error($"Invalid auto increment '{autoIncrement}'. Valid values are {VersionPartEx.ValidValues}");

        public static void ErrorInvalidMinMajorMinor(string minMajorMinor) =>
            Error($"Invalid minimum MAJOR.MINOR '{minMajorMinor}'. Valid values are {MajorMinor.ValidValues}");

        public static void ErrorInvalidVerbosity(string verbosity) =>
            Error($"Invalid verbosity '{verbosity}'. Valid values are {VerbosityMap.ValidValues}.");

        private static void Error(string message) => Message($"error : {message}");

        private static void Message(string message)
        {
            if (message.Contains('\r', StringComparison.OrdinalIgnoreCase) || message.Contains('\n', StringComparison.OrdinalIgnoreCase))
            {
                var lines = message.Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase).Split('\r', '\n');
                message = string.Join($"{Environment.NewLine}MinVer: ", lines);
            }

            Console.Error.WriteLine($"MinVer: {message}");
        }
    }
}
