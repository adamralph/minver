using System.IO;

namespace MinVerTests.Infra
{
    internal static class Solution
    {
        public static string GetFullPath(string path) =>
            Path.GetFullPath(Path.Combine(typeof(Solution).Assembly.Location, $"../../../../../", path));
    }
}
