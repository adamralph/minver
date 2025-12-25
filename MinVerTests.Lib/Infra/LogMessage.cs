namespace MinVerTests.Lib.Infra;

internal sealed class LogMessage
{
    public LogMessage(LogLevel level, string text, int code) => (Level, Text, Code) = (level, text, code);

    public LogLevel Level { get; }
    public string Text { get; }
    public int Code { get; }

    public override string ToString() => $"{Level}:".PadRight(7) + $"{(Code != 0 ? $"{Code}" : "")}{Text}";
}
