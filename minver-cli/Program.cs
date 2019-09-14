namespace MinVer
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Reflection;
    using McMaster.Extensions.CommandLineUtils;
    using MinVer.Lib;

    internal static class Program
    {
        private static readonly string informationalVersion = typeof(Versioner).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;

        private static int Main(string[] args)
        {
            var app = new CommandLineApplication { Name = "minver", FullName = $"MinVer CLI {informationalVersion}" };

            app.HelpOption();

            var autoIncrementOption = app.Option("-a|--auto-increment <VERSION_PART>", VersionPartEx.ValidValues, CommandOptionType.SingleValue);
            var buildMetaOption = app.Option("-b|--build-metadata <BUILD_METADATA>", "", CommandOptionType.SingleValue);
            var defaultPreReleasePhaseOption = app.Option("-d|--default-pre-release-phase <PHASE>", "alpha (default), preview, etc.", CommandOptionType.SingleValue);
            var minMajorMinorOption = app.Option("-m|--minimum-major-minor <MINIMUM_MAJOR_MINOR>", MajorMinor.ValidValues, CommandOptionType.SingleValue);
            var workDirOption = app.Option("-r|--repo <REPO>", "Working directory.", CommandOptionType.SingleValue);
            var tagPrefixOption = app.Option("-t|--tag-prefix <TAG_PREFIX>", "", CommandOptionType.SingleValue);
            var verbosityOption = app.Option("-v|--verbosity <VERBOSITY>", VerbosityMap.ValidValue, CommandOptionType.SingleValue);
#if MINVER
            var versionOverrideOption = app.Option("-o|--version-override <VERSION>", "", CommandOptionType.SingleValue);
#endif

            app.OnExecute(() =>
            {
                if (!TryParse(workDirOption.Value(), minMajorMinorOption.Value(), verbosityOption.Value(), autoIncrementOption.Value(), out var workDir, out var minMajorMinor, out var verbosity, out var autoIncrement))
                {
                    return 2;
                }

                var log = new Logger(verbosity);

                if (log.IsDebugEnabled)
                {
                    log.Debug($"MinVer {informationalVersion}.");
                }

#if MINVER
                Lib.Version version;
                if (!string.IsNullOrEmpty(versionOverrideOption.Value()))
                {
                    if (!Lib.Version.TryParse(versionOverrideOption.Value(), out version))
                    {
                        Logger.ErrorInvalidVersionOverride(versionOverrideOption.Value());
                        return 2;
                    }

                    log.Info($"Using version override {version}.");
                }
                else
                {
                    version = Versioner.GetVersion(workDir, tagPrefixOption.Value(), minMajorMinor, buildMetaOption.Value(), autoIncrement, defaultPreReleasePhaseOption.Value(), log);
                }
#else
                var version = Versioner.GetVersion(workDir, tagPrefixOption.Value(), minMajorMinor, buildMetaOption.Value(), autoIncrement, defaultPreReleasePhaseOption.Value(), log);
#endif

                Console.Out.WriteLine(version);

                return 0;
            });

            return app.Execute(args);
        }

        private static bool TryParse(string workDirOption, string minMajorMinorOption, string verbosityOption, string autoIncrementOption, out string workDir, out MajorMinor minMajorMinor, out Verbosity verbosity, out VersionPart autoIncrement)
        {
            workDir = ".";
            minMajorMinor = null;
            verbosity = default;
            autoIncrement = default;

            if (!string.IsNullOrEmpty(workDirOption) && !Directory.Exists(workDir = workDirOption))
            {
                Logger.ErrorWorkDirDoesNotExist(workDirOption);
                return false;
            }

            if (!string.IsNullOrEmpty(minMajorMinorOption) && !MajorMinor.TryParse(minMajorMinorOption, out minMajorMinor))
            {
                Logger.ErrorInvalidMinMajorMinor(minMajorMinorOption);
                return false;
            }

            if (!string.IsNullOrEmpty(verbosityOption) && !VerbosityMap.TryMap(verbosityOption, out verbosity))
            {
                Logger.ErrorInvalidVerbosity(verbosityOption);
                return false;
            }

            if (!string.IsNullOrEmpty(autoIncrementOption) && !Enum.TryParse(autoIncrementOption, true, out autoIncrement))
            {
                Logger.ErrorInvalidAutoIncrement(autoIncrementOption);
                return false;
            }

            return true;
        }
    }
}
