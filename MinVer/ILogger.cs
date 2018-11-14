using System;

namespace MinVer
{
    public interface ILogger
    {
        bool IsDebugEnabled { get; }

        void Debug(Func<string> createMessage);

        void Debug(string message);

        void Info(string message);
    }
}
