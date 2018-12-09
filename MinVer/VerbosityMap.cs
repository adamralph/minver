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

            void Add(Verbosity verbosity, int shortLength)
            {
                map.Add(verbosity.ToString(), verbosity);
                map.Add(verbosity.ToString().Substring(0, shortLength), verbosity);
            }
        }

        public static string ValidValue => "q[uiet], m[inimal], n[ormal], d[etailed], or diag[nostic] (case insensitive)";

        public static bool TryMap(string value, out Verbosity verbosity) => map.TryGetValue(value, out verbosity);
    }
}
