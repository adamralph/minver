using System;
using System.Diagnostics.CodeAnalysis;
#if MINVER_CLI
using System.Linq;
#endif
using MinVer.Lib;

namespace MinVer
{
    internal class Options
    {
        public Options(
            VersionPart autoIncrement,
            string buildMeta,
            string defaultPreReleasePhase,
            MajorMinor minMajorMinor,
            string tagPrefix,
            Verbosity verbosity,
            Lib.Version? versionOverride)
        {
            this.AutoIncrement = autoIncrement;
            this.BuildMeta = buildMeta;
            this.DefaultPreReleasePhase = defaultPreReleasePhase;
            this.MinMajorMinor = minMajorMinor;
            this.TagPrefix = tagPrefix;
            this.Verbosity = verbosity;
            this.VersionOverride = versionOverride;
        }

#if MINVER_CLI
        public static bool TryParseEnvVars([NotNullWhen(returnValue: true)] out Options? options)
        {
            options = null;

            var autoIncrement = default(VersionPart);
            var minMajorMinor = MajorMinor.Zero;
            var verbosity = default(Verbosity);
            var versionOverride = default(Lib.Version?);

            var autoIncrementEnvVar = GetEnvVar("MinVerAutoIncrement");
            if (!string.IsNullOrEmpty(autoIncrementEnvVar))
            {
                if (!Enum.TryParse(autoIncrementEnvVar, true, out autoIncrement))
                {
                    Logger.ErrorInvalidEnvVar("MinVerAutoIncrement", autoIncrementEnvVar, VersionPartExtensions.ValidValues);
                    return false;
                }
            }

            var buildMeta = GetEnvVar("MinVerBuildMetadata");
            var defaultPreReleasePhase = GetEnvVar("MinVerDefaultPreReleasePhase");

            var minMajorMinorEnvVar = GetEnvVar("MinVerMinimumMajorMinor");
            if (!string.IsNullOrEmpty(minMajorMinorEnvVar))
            {
                if (!MajorMinor.TryParse(minMajorMinorEnvVar, out minMajorMinor))
                {
                    Logger.ErrorInvalidEnvVar("MinVerMinimumMajorMinor", minMajorMinorEnvVar, MajorMinor.ValidValues);
                    return false;
                }
            }

            var tagPrefix = GetEnvVar("MinVerTagPrefix");

            var verbosityEnvVar = GetEnvVar("MinVerVerbosity");
            if (!string.IsNullOrEmpty(verbosityEnvVar))
            {
                if (!VerbosityMap.TryMap(verbosityEnvVar, out verbosity))
                {
                    Logger.ErrorInvalidEnvVar("MinVerVerbosity", verbosityEnvVar, VerbosityMap.ValidValues);
                    return false;
                }
            }

            var versionOverrideEnvVar = GetEnvVar("MinVerVersionOverride");
            if (!string.IsNullOrEmpty(versionOverrideEnvVar))
            {
                if (!Lib.Version.TryParse(versionOverrideEnvVar, out versionOverride))
                {
                    Logger.ErrorInvalidEnvVar("MinVerVersionOverride", versionOverrideEnvVar, "");
                    return false;
                }
            }

            options = new Options(autoIncrement, buildMeta, defaultPreReleasePhase, minMajorMinor, tagPrefix, verbosity, versionOverride);

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
                ? ""
                : (string?)vars[key] ?? "";
        }
#endif

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
            [NotNullWhen(returnValue: true)] out Options? options)
        {
            options = null;

            var autoIncrement = default(VersionPart);
            var minMajorMinor = MajorMinor.Zero;
            var verbosity = default(Verbosity);
            var versionOverride = default(Lib.Version?);

            if (!string.IsNullOrEmpty(autoIncrementOption) &&
                !Enum.TryParse(autoIncrementOption, true, out autoIncrement))
            {
                Logger.ErrorInvalidAutoIncrement(autoIncrementOption);
                return false;
            }

            if (!string.IsNullOrEmpty(minMajorMinorOption) &&
                !MajorMinor.TryParse(minMajorMinorOption, out minMajorMinor))
            {
                Logger.ErrorInvalidMinMajorMinor(minMajorMinorOption);
                return false;
            }

            if (!string.IsNullOrEmpty(verbosityOption) &&
                !VerbosityMap.TryMap(verbosityOption, out verbosity))
            {
                Logger.ErrorInvalidVerbosity(verbosityOption);
                return false;
            }

#if MINVER
            if (!string.IsNullOrEmpty(versionOverrideOption) &&
                !Lib.Version.TryParse(versionOverrideOption, out versionOverride))
            {
                Logger.ErrorInvalidVersionOverride(versionOverrideOption);
                return false;
            }
#endif

            options = new Options(autoIncrement, buildMetaOption, defaultPreReleasePhaseOption, minMajorMinor, tagPrefixOption, verbosity, versionOverride);

            return true;
        }

        public Options Mask(Options other) =>
            new Options(
                this.AutoIncrement == default ? other.AutoIncrement : this.AutoIncrement,
                string.IsNullOrEmpty(this.BuildMeta) ? other.BuildMeta : this.BuildMeta,
                string.IsNullOrEmpty(this.DefaultPreReleasePhase) ? other.DefaultPreReleasePhase : this.DefaultPreReleasePhase,
                this.MinMajorMinor == MajorMinor.Zero ? other.MinMajorMinor : this.MinMajorMinor,
                string.IsNullOrEmpty(this.TagPrefix) ? other.TagPrefix : this.TagPrefix,
                this.Verbosity == default ? other.Verbosity : this.Verbosity,
                this.VersionOverride ?? other.VersionOverride);

        public VersionPart AutoIncrement { get; private set; }

        public string BuildMeta { get; private set; }

        public string DefaultPreReleasePhase { get; private set; }

        public MajorMinor MinMajorMinor { get; private set; }

        public string TagPrefix { get; private set; }

        public Verbosity Verbosity { get; private set; }

        public Lib.Version? VersionOverride { get; private set; }
    }
}
