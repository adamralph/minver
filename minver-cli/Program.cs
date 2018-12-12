namespace MinVer
{
    using System;
    using System.Linq;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;
    using MinVer.Lib;
    using Version = MinVer.Lib.Version;
    using System.Reflection;
    using System.Collections.Generic;

    internal static class Program
    {
        private static readonly string informationalVersion = typeof(Versioner).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;

        private static int Main(string[] args)
        {
            if (args.Contains("--major-minor", StringComparer.OrdinalIgnoreCase))
            {
                Console.Out.WriteLine("--major-minor has been renamed to --minimum-major-minor");
                return 2;
            }

            var app = new CommandLineApplication { Name = "minver", FullName = $"MinVer CLI {informationalVersion}" };

            app.HelpOption();

            var buildMetaOption = app.Option("-b|--build-metadata <BUILD_METADATA>", "", CommandOptionType.SingleValue);
            var minMajorMinorOption = app.Option("-m|--minimum-major-minor <MINIMUM_MAJOR_MINOR>", MajorMinor.ValidValues, CommandOptionType.SingleValue);
            var defaultPrereleaseIdentifiersOption = app.Option("-p|--default-prerelease-identifiers <DEFAULT_PRERELEASE_IDENTIFIERS>", "The pre-release identifiers to use when the latest tag is a RTM. The default is \"alpha.0\"", CommandOptionType.SingleValue);
            var repoOrWorkDirOption = app.Option("-r|--repo <REPO>", "Repository or working directory.", CommandOptionType.SingleValue);
            var tagPrefixOption = app.Option("-t|--tag-prefix <TAG_PREFIX>", "", CommandOptionType.SingleValue);
            var verbosityOption = app.Option("-v|--verbosity <VERBOSITY>", VerbosityMap.ValidValue, CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!TryParse(repoOrWorkDirOption.Value(), minMajorMinorOption.Value(), verbosityOption.Value(), defaultPrereleaseIdentifiersOption.Value(), out var repoOrWorkDir, out var minMajorMinor, out var verbosity, out var defaultPrereleaseIdentifiers))
                {
                    return 2;
                }

                var version = GetVersion(repoOrWorkDir, tagPrefixOption.Value(), minMajorMinor, defaultPrereleaseIdentifiers, buildMetaOption.Value(), verbosity);

                Console.Out.WriteLine(version);

                return 0;
            });

            return app.Execute(args);
        }

        private static bool TryParse(string repoOrWorkDirOption, string minMajorMinorOption, string verbosityOption, string defaultPrereleaseIdentifiersOption, out string repoOrWorkDir, out MajorMinor minMajorMinor, out Verbosity verbosity, out IEnumerable<string> defaultPrereleaseIdentifiers)
        {
            repoOrWorkDir = ".";
            minMajorMinor = default;
            verbosity = default;
            defaultPrereleaseIdentifiers = null;

            if (!string.IsNullOrEmpty(repoOrWorkDirOption) && !Directory.Exists(repoOrWorkDir = repoOrWorkDirOption))
            {
                Logger.ErrorRepoOrWorkDirDoesNotExist(repoOrWorkDirOption);
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

            if (!string.IsNullOrEmpty(defaultPrereleaseIdentifiersOption))
            {
                defaultPrereleaseIdentifiers = defaultPrereleaseIdentifiersOption.Split('.', StringSplitOptions.RemoveEmptyEntries);
            }

            return true;
        }

        private static Version GetVersion(string repoOrWorkDir, string tagPrefix, MajorMinor minMajorMinor, IEnumerable<string> defaultPrereleaseIdentifiers, string buildMeta, Verbosity verbosity)
        {
            var log = new Logger(verbosity);

            if (log.IsDebugEnabled)
            {
                log.Debug($"MinVer {informationalVersion}.");
            }

            if (!RepositoryEx.TryCreateRepo(repoOrWorkDir, out var repo))
            {
                var version = new Version(minMajorMinor?.Major ?? 0, minMajorMinor?.Minor ?? 0, defaultPrereleaseIdentifiers, buildMeta);

                log.WarnIsNotAValidRepositoryOrWorkDirUsingDefaultVersion(repoOrWorkDir, version);

                return version;
            }

            try
            {
                return Versioner.GetVersion(repo, tagPrefix, minMajorMinor, defaultPrereleaseIdentifiers, buildMeta, log);
            }
            finally
            {
                repo.Dispose();
            }
        }
    }
}
