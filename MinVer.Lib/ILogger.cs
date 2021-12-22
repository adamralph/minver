namespace MinVer.Lib
{
    public interface ILogger
    {
        bool IsTraceEnabled { get; }

        bool IsDebugEnabled { get; }

        bool IsInfoEnabled { get; }

        bool IsWarnEnabled { get; }

        bool Trace(string message);

        bool Debug(string message);

        bool Info(string message);

        bool Warn(int code, string message);
    }
}
