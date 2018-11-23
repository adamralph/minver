namespace MinVer
{
    using System;
    using MinVer.Lib;
    using Version = MinVer.Lib.Version;

    internal class Logger : ILogger
    {
        private readonly Verbosity level;

        public Logger(Verbosity level) => this.level = level;

        public bool IsTraceEnabled => this.level >= Verbosity.Trace;

        public bool IsDebugEnabled => this.level >= Verbosity.Debug;

        public void Trace(string message)
        {
            if (this.level >= Verbosity.Trace)
            {
                Message(message);
            }
        }

        public void Debug(Func<string> createMessage)
        {
            if (this.level >= Verbosity.Debug)
            {
                Message(createMessage());
            }
        }

        public void Debug(string message)
        {
            if (this.level >= Verbosity.Debug)
            {
                Message(message);
            }
        }

        public void Info(string message)
        {
            if (this.level >= Verbosity.Info)
            {
                Message(message);
            }
        }

        public void WarnInvalidRepoPath(string path, Version version) =>
            this.Warn($"'{path}' is not a valid repository or working directory. Using default version: {version}");

        public static void ErrorInvalidRepoPath(string path) =>
            Error($"Invalid repository path '{path}'. Directory does not exist.");

        public static void ErrorInvalidMajorMinorRange(string majorMinor) =>
            Error($"Invalid MAJOR.MINOR range '{majorMinor}'.");

        public static void ErrorInvalidVerbosityLevel(string verbosity) =>
            Error($"Invalid verbosity level '{verbosity}'. Valid levels are error, warn, info, debug, and trace.");

        private void Warn(string message)
        {
            if (this.level >= Verbosity.Warn)
            {
                Message($"warning : {message}");
            }
        }

        private static void Error(string message) => Message($"error : {message}");

        private static void Message(string message) => Console.Error.WriteLine($"MinVer: {message}");
    }
}
