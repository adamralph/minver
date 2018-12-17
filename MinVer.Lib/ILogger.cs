namespace MinVer.Lib
{
    public interface ILogger
    {
        bool IsTraceEnabled { get; }

        bool IsDebugEnabled { get; }

        void Trace(string message);

        void Debug(string message);

        void Info(string message);

        void Warn(int code, string message);
    }
}
