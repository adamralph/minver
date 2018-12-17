namespace MinVerTests.Infra
{
    using System;
    using MinVer.Lib;

    internal class TestLogger : ILogger
    {
        public bool IsTraceEnabled => false;

        public bool IsDebugEnabled => false;

        public void Trace(string message)
        {
        }

        public void Debug(Func<string> createMessage)
        {
        }

        public void Debug(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Warn(int code, string message)
        {
        }
    }
}
