namespace MinVer
{
    using System;
    using MinVer.Lib;

    internal class Options
    {
        public static bool TryParse(
            string autoIncrementOption,
            string buildMetaOption,
            string defaultPreReleasePhaseOption,
            string minMajorMinorOption,
            string tagPrefixOption,
            string verbosityOption,
#if MINVER
            string versionOverrideOption,
#endif
            out Options options)
        {
            options = new Options();

            if (!string.IsNullOrEmpty(autoIncrementOption))
            {
                if (!Enum.TryParse<VersionPart>(autoIncrementOption, true, out var autoIncrement))
                {
                    Logger.ErrorInvalidAutoIncrement(autoIncrementOption);
                    return false;
                }

                options.AutoIncrement = autoIncrement;
            }

            options.BuildMeta = buildMetaOption;
            options.DefaultPreReleasePhase = defaultPreReleasePhaseOption;

            if (!string.IsNullOrEmpty(minMajorMinorOption))
            {
                if (!MajorMinor.TryParse(minMajorMinorOption, out var minMajorMinor))
                {
                    Logger.ErrorInvalidMinMajorMinor(minMajorMinorOption);
                    return false;
                }

                options.MinMajorMinor = minMajorMinor;
            }

            options.TagPrefix = tagPrefixOption;

            if (!string.IsNullOrEmpty(verbosityOption))
            {
                if (!VerbosityMap.TryMap(verbosityOption, out var verbosity))
                {
                    Logger.ErrorInvalidVerbosity(verbosityOption);
                    return false;
                }

                options.Verbosity = verbosity;
            }

#if MINVER
            if (!string.IsNullOrEmpty(versionOverrideOption))
            {
                if (!Lib.Version.TryParse(versionOverrideOption, out var versionOverride))
                {
                    Logger.ErrorInvalidVersionOverride(versionOverrideOption);
                    return false;
                }

                options.VersionOverride = versionOverride;
            }
#endif

            return true;
        }

        public VersionPart AutoIncrement { get; private set; }

        public string BuildMeta { get; private set; }

        public string DefaultPreReleasePhase { get; private set; }

        public MajorMinor MinMajorMinor { get; private set; }

        public string TagPrefix { get; private set; }

        public Verbosity Verbosity { get; private set; }

#if MINVER
        public Lib.Version VersionOverride { get; private set; }
#endif
    }
}
