using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MinVer
{
    internal static class VerbosityMap
    {
        private static readonly Dictionary<string, Verbosity?> map = new Dictionary<string, Verbosity?>(StringComparer.OrdinalIgnoreCase);

        static VerbosityMap()
        {
            Add(Verbosity.Error);
            Add(Verbosity.Warn);
            Add(Verbosity.Info);
            Add(Verbosity.Debug);
            Add(Verbosity.Trace);

            static void Add(Verbosity verbosity)
            {
                map.Add(verbosity.ToString(), verbosity);
                map.Add(verbosity.ToString()[..1], verbosity);
            }
        }

        // spell-checker:disable
        public static string ValidValues => "e[rror], w[arn], i[nfo] (default), d[ebug], or t[race] (case insensitive)";

        // spell-checker:enable
        public static bool TryMap(string value, [NotNullWhen(returnValue: true)] out Verbosity? verbosity) => map.TryGetValue(value, out verbosity);
    }
}
