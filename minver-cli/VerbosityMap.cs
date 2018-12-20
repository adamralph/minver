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

            void Add(Verbosity verbosity)
            {
                map.Add(verbosity.ToString(), verbosity);
                map.Add(verbosity.ToString().Substring(0, 1), verbosity);
            }
        }

        public static string ValidValue => "e[rror], w[arn], i[nfo] (default), d[ebug], or t[race] (case insensitive)";

        public static bool TryMap(string value, out Verbosity verbosity) => map.TryGetValue(value, out verbosity);
    }
}
