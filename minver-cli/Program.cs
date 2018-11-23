namespace MinVer
{
    using System;
    using System.Linq;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;
    using MinVer.Lib;
    using Version = MinVer.Lib.Version;

    internal static class Program
    {
        private static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            var levels = Enum.GetValues(typeof(Verbosity)).Cast<Verbosity>().OrderBy(_ => _).Select(level => level.ToString().ToLowerInvariant()).ToList();

            var buildMetadata = app.Option("-b|--build-metadata <BUILD_METADATA>", "", CommandOptionType.SingleValue);
            var majorMinor = app.Option("-m|--major-minor <RANGE>", "", CommandOptionType.SingleValue);
            var repo = app.Option("-r|--repo <PATH>", "Repository or working directory.", CommandOptionType.SingleValue);
            var tagPrefix = app.Option("-t|--tag-prefix <TAG_PREFIX>", "", CommandOptionType.SingleValue);
            var verbosity = app.Option("-v|--verbosity <LEVEL>", $"{string.Join(", ", levels.Take(levels.Count - 1))}, or {levels.Last()}", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!TryParse(repo.Value(), majorMinor.Value(), verbosity.Value(), out var path, out var range, out var level))
                {
                    return 2;
                }

                var version = GetVersion(path, tagPrefix.Value(), range, buildMetadata.Value(), level);

                Console.Out.WriteVersion(version);

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

            if (!string.IsNullOrEmpty(verbosity) && !Enum.TryParse(verbosity, true, out level))
            {
                Logger.ErrorInvalidVerbosityLevel(verbosity);
                return false;
            }

            return true;
        }

        private static Version GetVersion(string path, string tagPrefix, MajorMinor range, string buildMetadata, Verbosity level)
        {
            var log = new Logger(level);

            if (!RepositoryEx.TryCreateRepo(path, out var repo))
            {
                var version = new Version();

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
