using System.Diagnostics.CodeAnalysis;

namespace MinVer;

internal static class VerbosityMap
{
#if NET10_0
    private static readonly Dictionary<string, Verbosity?> Map = new(StringComparer.OrdinalIgnoreCase);
#else
    private static readonly Dictionary<string, Verbosity?> Map = [with(StringComparer.OrdinalIgnoreCase)];
#endif

    static VerbosityMap()
    {
        Add(Verbosity.Error);
        Add(Verbosity.Warn);
        Add(Verbosity.Info);
        Add(Verbosity.Debug);
        Add(Verbosity.Trace);

        static void Add(Verbosity verbosity)
        {
            Map.Add(verbosity.ToString(), verbosity);
            Map.Add(verbosity.ToString()[..1], verbosity);
        }
    }

    // spell-checker:disable
    public static string ValidValues => "e[rror], w[arn], i[nfo] (default), d[ebug], or t[race] (case insensitive)";

    // spell-checker:enable
    public static bool TryMap(string value, [NotNullWhen(returnValue: true)] out Verbosity? verbosity) => Map.TryGetValue(value, out verbosity);
}
