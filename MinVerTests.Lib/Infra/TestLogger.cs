using System.Collections.Generic;
using MinVer.Lib;

namespace MinVerTests.Lib.Infra
{
    public class TestLogger : ILogger
    {
#if NET5_0_OR_GREATER
        private readonly List<string> debugMessages = new();
#else
        private readonly List<string> debugMessages = new List<string>();
#endif

        public bool IsTraceEnabled => true;

        public bool IsDebugEnabled => true;

        public IEnumerable<string> DebugMessages => this.debugMessages;

        public void Trace(string message)
        {
        }

        public void Debug(string message) => this.debugMessages.Add(message);

        public void Info(string message)
        {
        }

        public void Warn(int code, string message)
        {
        }
    }
}
