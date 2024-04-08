namespace MinVerTests.Infra;

public static class StringExtensions
{
#if NET8_0_OR_GREATER
    private static readonly char[] newLineChars = ['\r', '\n',];
#else
    private static readonly char[] newLineChars = { '\r', '\n', };
#endif

    public static string[] ToNonEmptyLines(this string text) =>
#pragma warning disable CA1062 // Validate arguments of public methods
        text.Split(newLineChars, StringSplitOptions.RemoveEmptyEntries);
#pragma warning restore CA1062 // Validate arguments of public methods

    public static string ToAltCase(this string value) =>
#pragma warning disable CA1308 // Normalize strings to uppercase
        new(value.Select((c, i) => i % 2 == 0 ? c.ToString().ToLowerInvariant()[0] : c.ToString().ToUpperInvariant()[0]).ToArray());
#pragma warning restore CA1308 // Normalize strings to uppercase
}
