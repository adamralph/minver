namespace MinVer.Lib
{
    internal class NullLogger : ILogger
    {
        public bool IsTraceEnabled => false;

        public bool IsDebugEnabled => false;

        public void Debug(string message) { }

        public void Info(string message) { }

        public void Trace(string message) { }

        public void Warn(int code, string message) { }
    }
}
