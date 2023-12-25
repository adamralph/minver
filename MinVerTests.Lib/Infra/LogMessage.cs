namespace MinVerTests.Lib.Infra;

internal sealed class LogMessage
{
    public LogMessage(LogLevel level, string text, int code) => (this.Level, this.Text, this.Code) = (level, text, code);

    public LogLevel Level { get; }
    public string Text { get; }
    public int Code { get; }

    public override string ToString() => $"{this.Level}:".PadRight(7) + $"{(this.Code != 0 ? $"{this.Code}" : "")}{this.Text}";
}
