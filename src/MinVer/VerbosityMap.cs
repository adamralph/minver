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
        Add(Verbosity.Quiet, 1);
        Add(Verbosity.Minimal, 1);
        Add(Verbosity.Normal, 1);
        Add(Verbosity.Detailed, 1);
        Add(Verbosity.Diagnostic, 4);

        static void Add(Verbosity verbosity, int shortLength)
        {
            Map.Add(verbosity.ToString(), verbosity);
            Map.Add(verbosity.ToString()[..shortLength], verbosity);
        }
    }

    // spell-checker:disable
    public static string ValidValues => "q[uiet], m[inimal] (default), n[ormal], d[etailed], or diag[nostic] (case insensitive)";

    // spell-checker:enable
    public static bool TryMap(string value, [NotNullWhen(returnValue: true)] out Verbosity? verbosity) => Map.TryGetValue(value, out verbosity);
}
