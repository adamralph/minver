namespace MinVerTests.Infra
{
    using System;
    using MinVer.Lib;

    internal class TestLogger : ILogger
    {
        public bool IsDebugEnabled => true;

        public void Debug(Func<string> createMessage)
        {
        }

        public void Debug(string message)
        {
        }

        public void Info(string message)
        {
        }
    }
}
