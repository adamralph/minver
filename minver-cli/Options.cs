using System;
#if MINVER_CLI
using System.Linq;
#endif
using MinVer.Lib;

namespace MinVer
{
    internal class Options
    {
#if MINVER_CLI
        public static bool TryParseEnvVars(out Options options)
        {
            options = new Options();

            var autoIncrementEnvVar = GetEnvVar("MinVerAutoIncrement");
            if (!string.IsNullOrEmpty(autoIncrementEnvVar))
            {
                if (!Enum.TryParse<VersionPart>(autoIncrementEnvVar, true, out var autoIncrement))
                {
                    Logger.ErrorInvalidEnvVar("MinVerAutoIncrement", autoIncrementEnvVar, VersionPartExtensions.ValidValues);
                    return false;
                }

                options.AutoIncrement = autoIncrement;
            }

            options.BuildMeta = GetEnvVar("MinVerBuildMetadata");
            options.DefaultPreReleasePhase = GetEnvVar("MinVerDefaultPreReleasePhase");

            var minMajorMinorEnvVar = GetEnvVar("MinVerMinimumMajorMinor");
            if (!string.IsNullOrEmpty(minMajorMinorEnvVar))
            {
                if (!MajorMinor.TryParse(minMajorMinorEnvVar, out var minMajorMinor))
                {
                    Logger.ErrorInvalidEnvVar("MinVerMinimumMajorMinor", minMajorMinorEnvVar, MajorMinor.ValidValues);
                    return false;
                }

                options.MinMajorMinor = minMajorMinor;
            }

            options.TagPrefix = GetEnvVar("MinVerTagPrefix");

            var verbosityEnvVar = GetEnvVar("MinVerVerbosity");
            if (!string.IsNullOrEmpty(verbosityEnvVar))
            {
                if (!VerbosityMap.TryMap(verbosityEnvVar, out var verbosity))
                {
                    Logger.ErrorInvalidEnvVar("MinVerVerbosity", verbosityEnvVar, VerbosityMap.ValidValues);
                    return false;
                }

                options.Verbosity = verbosity;
            }

            var versionOverrideEnvVar = GetEnvVar("MinVerVersionOverride");
            if (!string.IsNullOrEmpty(versionOverrideEnvVar))
            {
                if (!Lib.Version.TryParse(versionOverrideEnvVar, out var versionOverride))
                {
                    Logger.ErrorInvalidEnvVar("MinVerVersionOverride", versionOverrideEnvVar, null);
                    return false;
                }

                options.VersionOverride = versionOverride;
            }

            return true;
        }

        private static string GetEnvVar(string name)
        {
            var vars = Environment.GetEnvironmentVariables();

            var key = vars.Keys
                .Cast<string>()
                .OrderBy(_ => _)
                .FirstOrDefault(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));

            return key == null
                ? null
                : (string)vars[key];
        }
#endif

        public static bool TryParse(
            string autoIncrementOption,
            string buildMetaOption,
            string defaultPreReleasePhaseOption,
            string minMajorMinorOption,
            string tagPrefixOption,
            string verbosityOption,
            bool writeGitHubActionOutput,
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

            options.ShowGitHubActionOutput = writeGitHubActionOutput;

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

        public Options Mask(Options other) =>
            new Options
            {
                AutoIncrement = this.AutoIncrement == default ? other.AutoIncrement : this.AutoIncrement,
                BuildMeta = this.BuildMeta ?? other.BuildMeta,
                DefaultPreReleasePhase = this.DefaultPreReleasePhase ?? other.DefaultPreReleasePhase,
                MinMajorMinor = this.MinMajorMinor ?? other.MinMajorMinor,
                TagPrefix = this.TagPrefix ?? other.TagPrefix,
                Verbosity = this.Verbosity == default ? other.Verbosity : this.Verbosity,
                VersionOverride = this.VersionOverride ?? other.VersionOverride,
                ShowGitHubActionOutput = this.ShowGitHubActionOutput ? this.ShowGitHubActionOutput : other.ShowGitHubActionOutput,
            };

        public VersionPart AutoIncrement { get; private set; }

        public string BuildMeta { get; private set; }

        public string DefaultPreReleasePhase { get; private set; }

        public MajorMinor MinMajorMinor { get; private set; }

        public string TagPrefix { get; private set; }

        public Verbosity Verbosity { get; private set; }

        public Lib.Version VersionOverride { get; private set; }

        public bool ShowGitHubActionOutput { get; private set; }
    }
}
