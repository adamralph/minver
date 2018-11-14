namespace MinVer
{
    using System;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;

    internal class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            var buildMetadata = app.Option("--build-metadata <BUILD_METADATA>", "", CommandOptionType.SingleValue);
            var majorMinor = app.Option("--major-minor <RANGE>", "", CommandOptionType.SingleValue);
            var repo = app.Option("--repo <PATH>", "", CommandOptionType.SingleValue);
            var tagPrefix = app.Option("--tag-prefix <TAG_PREFIX>", "", CommandOptionType.SingleValue);
            var verbosity = app.Option("--verbosity <LEVEL>", "", CommandOptionType.SingleValue);

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

            if (!string.IsNullOrEmpty(verbosity) && !Enum.TryParse(verbosity, true, out level))
            {
                Logger.ErrorInvalidVerbosityLevel(verbosity);
                return false;
            }

            return true;
        }

        public static Version GetVersion(string path, string tagPrefix, MajorMinor range, string buildMetadata, Verbosity level)
        {
            if (!RepositoryEx.TryCreateRepo(path, out var repo))
            {
                var version = new Version();

                Logger.WarnInvalidRepoPath(path, version);

                return version;
            }

            try
            {
                return Versioner.GetVersion(repo, tagPrefix, range, buildMetadata, new Logger(level));
            }
            finally
            {
                repo.Dispose();
            }
        }
    }
}
