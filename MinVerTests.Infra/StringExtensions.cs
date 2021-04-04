using System.Linq;

namespace MinVerTests.Infra
{
    public static class StringExtensions
    {
        public static string ToAltCase(this string value) =>
#if NET5_0_OR_GREATER
            new(value.Select((c, i) => i % 2 == 0 ? c.ToString().ToLowerInvariant()[0] : c.ToString().ToUpperInvariant()[0]).ToArray());
#else
            new string(value.Select((c, i) => i % 2 == 0 ? c.ToString().ToLowerInvariant()[0] : c.ToString().ToUpperInvariant()[0]).ToArray());
#endif
    }
}
