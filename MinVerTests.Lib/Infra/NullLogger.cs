using MinVer.Lib;

namespace MinVerTests.Lib.Infra;

internal sealed class NullLogger : ILogger
{
    public static readonly NullLogger Instance =
        new();

    private NullLogger()
    {
    }

    public bool IsTraceEnabled => false;

    public bool IsDebugEnabled => false;

    public bool IsInfoEnabled => false;

    public bool IsWarnEnabled => false;

    public bool Trace(string message) => false;

    public bool Debug(string message) => false;

    public bool Info(string message) => false;

    public bool Warn(int code, string message) => false;
}
