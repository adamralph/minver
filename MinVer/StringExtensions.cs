namespace MinVer
{
    internal static class StringExtensions
    {
        public static string Before(this string text, int? separator) => separator.HasValue ? text?.Substring(0, separator.Value) : text;

        public static string After(this string text, int? separator) => separator.HasValue ? text?.Substring(separator.Value + 1) : default;
    }
}
