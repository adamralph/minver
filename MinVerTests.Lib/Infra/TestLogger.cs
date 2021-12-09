using System.Collections.Generic;
using MinVer.Lib;

namespace MinVerTests.Lib.Infra
{
    internal class TestLogger : ILogger
    {
#if NET5_0_OR_GREATER
        private readonly List<LogMessage> messages = new();
#else
        private readonly List<LogMessage> messages = new List<LogMessage>();
#endif

        public bool IsTraceEnabled => true;

        public bool IsDebugEnabled => true;

        public IEnumerable<LogMessage> Messages => this.messages;

        public void Trace(string message) => this.messages.Add(new LogMessage(LogLevel.Trace, message, 0));

        public void Debug(string message) => this.messages.Add(new LogMessage(LogLevel.Debug, message, 0));

        public void Info(string message) => this.messages.Add(new LogMessage(LogLevel.Info, message, 0));

        public void Warn(int code, string message) => this.messages.Add(new LogMessage(LogLevel.Warn, message, 0));
    }
}
