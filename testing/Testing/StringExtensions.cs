namespace Testing;

public static class StringExtensions
{
    private static readonly char[] NewLineChars = ['\r', '\n',];

    public static string[] ToNonEmptyLines(this string text) =>
        text.Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);

    public static string ToAltCase(this string value) =>
#pragma warning disable CA1308 // Normalize strings to uppercase
        new([.. value.Select((c, i) => i % 2 == 0 ? c.ToString().ToLowerInvariant()[0] : c.ToString().ToUpperInvariant()[0]),]);
#pragma warning restore CA1308 // Normalize strings to uppercase
}
