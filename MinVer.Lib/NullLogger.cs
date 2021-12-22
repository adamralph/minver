namespace MinVer.Lib
{
    internal class NullLogger : ILogger
    {
        public bool IsTraceEnabled => false;

        public bool IsDebugEnabled => false;

        public bool IsInfoEnabled => false;

        public bool IsWarnEnabled => false;

        public bool Debug(string message) => false;

        public bool Info(string message) => false;

        public bool Trace(string message) => false;

        public bool Warn(int code, string message) => false;
    }
}
