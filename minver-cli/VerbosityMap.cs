namespace MinVer
{
    using System;
    using System.Collections.Generic;

    internal static class VerbosityMap
    {
        private static readonly Dictionary<string, Verbosity> map = new Dictionary<string, Verbosity>(StringComparer.OrdinalIgnoreCase);

        static VerbosityMap()
        {
            Add(Verbosity.Error);
            Add(Verbosity.Warn);
            Add(Verbosity.Info);
            Add(Verbosity.Debug);
            Add(Verbosity.Trace);

            void Add(Verbosity level)
            {
                map.Add(level.ToString(), level);
                map.Add(level.ToString().Substring(0, 1), level);
            }
        }

        public static string Levels => "e[rror], w[arn], i[nfo], d[ebug], or t[race] (case insensitive)";

        public static bool TryMap(string text, out Verbosity level) => map.TryGetValue(text, out level);
    }
}
