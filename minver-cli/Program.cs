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
            var app = new CommandLineApplication { Name = "minver", FullName = $"MinVer CLI {informationalVersion}" };

            app.HelpOption();

            var buildMetadata = app.Option("-b|--build-metadata <BUILD_METADATA>", "", CommandOptionType.SingleValue);
            var majorMinor = app.Option("-m|--major-minor <RANGE>", "1.0, 1.1, 2.0, etc.", CommandOptionType.SingleValue);
            var repo = app.Option("-r|--repo <PATH>", "Repository or working directory.", CommandOptionType.SingleValue);
            var tagPrefix = app.Option("-t|--tag-prefix <TAG_PREFIX>", "", CommandOptionType.SingleValue);
            var verbosity = app.Option("-v|--verbosity <LEVEL>", VerbosityMap.Levels, CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!TryParse(repo.Value(), majorMinor.Value(), verbosity.Value(), out var path, out var range, out var level))
                {
                    return 2;
                }

                var version = GetVersion(path, tagPrefix.Value(), range, buildMetadata.Value(), level);

                Console.Out.WriteLine(version);

                return 0;
            });

            return app.Execute(args);
        }

        private static bool TryParse(string repo, string majorMinor, string verbosity, out string path, out MajorMinor range, out Verbosity level)
        {
            path = ".";
            range = default;
            level = default;

            if (!string.IsNullOrEmpty(repo) && !Directory.Exists(path = repo))
            {
                Logger.ErrorInvalidRepoPath(path);
                return false;
            }

            if (!string.IsNullOrEmpty(majorMinor) && !MajorMinor.TryParse(majorMinor, out range))
            {
                Logger.ErrorInvalidMajorMinorRange(majorMinor);
                return false;
            }

            if (!string.IsNullOrEmpty(verbosity) && !VerbosityMap.TryMap(verbosity, out level))
            {
                Logger.ErrorInvalidVerbosityLevel(verbosity);
                return false;
            }

            return true;
        }

        private static Version GetVersion(string path, string tagPrefix, MajorMinor range, string buildMetadata, Verbosity level)
        {
            var log = new Logger(level);

            if (log.IsDebugEnabled)
            {
                log.Debug($"MinVer {informationalVersion}.");
            }

            if (!RepositoryEx.TryCreateRepo(path, out var repo))
            {
                var version = new Version(range?.Major ?? 0, range?.Minor ?? 0, buildMetadata);

                log.WarnInvalidRepoPath(path, version);

                return version;
            }

            try
            {
                return Versioner.GetVersion(repo, tagPrefix, range, buildMetadata, log);
            }
            finally
            {
                repo.Dispose();
            }
        }
    }
}
