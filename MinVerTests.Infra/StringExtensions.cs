using System.Linq;

namespace MinVerTests.Infra
{
    public static class StringExtensions
    {
        public static string ToAltCase(this string value) =>
            new string(value.Select((c, i) => i % 2 == 0 ? c.ToString().ToLowerInvariant()[0] : c.ToString().ToUpperInvariant()[0]).ToArray());
    }
}
