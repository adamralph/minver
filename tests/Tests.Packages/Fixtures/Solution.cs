namespace Tests.Packages.Fixtures;

public static class Solution
{
    private static readonly Lazy<string> Directory = new(() =>
    {
        var candidate = new DirectoryInfo(AppContext.BaseDirectory);
        while (candidate is not null && candidate.GetFiles("*.slnx").Length == 0)
        {
            candidate = candidate.Parent;
        }

        return candidate?.FullName ?? throw new InvalidOperationException("Solution directory not found.");
    });

    public static string GetFullPath(string path) => Path.Combine(Directory.Value, path);

    public const string Configuration =
#if DEBUG
        "Debug";
#elif RELEASE
        "Release";
#endif
}
