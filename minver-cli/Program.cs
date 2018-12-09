namespace MinVer
{
    using System;
    using System.Linq;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;
    using MinVer.Lib;
    using Version = MinVer.Lib.Version;
    using System.Reflection;

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

            var buildMetadata = app.Option("-b|--build-metadata <BUILD_METADATA>", "", CommandOptionType.SingleValue);
            var minimumMajorMinor = app.Option("-m|--minimum-major-minor <RANGE>", "1.0, 1.1, 2.0, etc.", CommandOptionType.SingleValue);
            var repo = app.Option("-r|--repo <PATH>", "Repository or working directory.", CommandOptionType.SingleValue);
            var tagPrefix = app.Option("-t|--tag-prefix <TAG_PREFIX>", "", CommandOptionType.SingleValue);
            var verbosity = app.Option("-v|--verbosity <LEVEL>", VerbosityMap.Levels, CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!TryParse(repo.Value(), minimumMajorMinor.Value(), verbosity.Value(), out var path, out var minimumRange, out var level))
                {
                    return 2;
                }

                var version = GetVersion(path, tagPrefix.Value(), minimumRange, buildMetadata.Value(), level);

                Console.Out.WriteLine(version);

                return 0;
            });

            return app.Execute(args);
        }

        private static bool TryParse(string repo, string minimumMajorMinor, string verbosity, out string path, out MajorMinor minimumRange, out Verbosity level)
        {
            path = ".";
            minimumRange = default;
            level = default;

            if (!string.IsNullOrEmpty(repo) && !Directory.Exists(path = repo))
            {
                Logger.ErrorInvalidRepoPath(path);
                return false;
            }

            if (!string.IsNullOrEmpty(minimumMajorMinor) && !MajorMinor.TryParse(minimumMajorMinor, out minimumRange))
            {
                Logger.ErrorInvalidMinimumMajorMinor(minimumMajorMinor);
                return false;
            }

            if (!string.IsNullOrEmpty(verbosity) && !VerbosityMap.TryMap(verbosity, out level))
            {
                Logger.ErrorInvalidVerbosityLevel(verbosity);
                return false;
            }

            return true;
        }

        private static Version GetVersion(string path, string tagPrefix, MajorMinor minimumRange, string buildMetadata, Verbosity level)
        {
            var log = new Logger(level);

            if (log.IsDebugEnabled)
            {
                log.Debug($"MinVer {informationalVersion}.");
            }

            if (!RepositoryEx.TryCreateRepo(path, out var repo))
            {
                var version = new Version(minimumRange?.Major ?? 0, minimumRange?.Minor ?? 0, buildMetadata);

                log.WarnInvalidRepoPath(path, version);

                return version;
            }

            try
            {
                return Versioner.GetVersion(repo, tagPrefix, minimumRange, buildMetadata, log);
            }
            finally
            {
                repo.Dispose();
            }
        }
    }
}
