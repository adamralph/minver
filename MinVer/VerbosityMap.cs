namespace MinVer
{
    using System;
    using System.Collections.Generic;

    internal static class VerbosityMap
    {
        private static readonly Dictionary<string, Verbosity> map = new Dictionary<string, Verbosity>(StringComparer.OrdinalIgnoreCase);

        static VerbosityMap()
        {
            Add(Verbosity.Quiet, 1);
            Add(Verbosity.Minimal, 1);
            Add(Verbosity.Normal, 1);
            Add(Verbosity.Detailed, 1);
            Add(Verbosity.Diagnostic, 4);

            void Add(Verbosity level, int shortLength)
            {
                map.Add(level.ToString(), level);
                map.Add(level.ToString().Substring(0, shortLength), level);
            }
        }

        public static string Levels => "q[uiet], m[inimal], n[ormal], d[etailed], or diag[nostic] (case insensitive)";

        public static bool TryMap(string text, out Verbosity level) => map.TryGetValue(text, out level);
    }
}
