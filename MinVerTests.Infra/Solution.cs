namespace MinVerTests.Infra;

public static class Solution
{
    public static string GetFullPath(string path) =>
        Path.GetFullPath(Path.Combine(typeof(Solution).Assembly.Location, "../../../../../", path));

    public const string Configuration =
#if DEBUG
        "Debug";
#elif RELEASE
        "Release";
#endif
}
