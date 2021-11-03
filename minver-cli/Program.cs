using System;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using MinVer.Lib;

namespace MinVer
{
    internal static class Program
    {
        private static readonly string informationalVersion = typeof(Versioner).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;

        private static int Main(string[] args)
        {
            using var app = new CommandLineApplication { Name = "minver", FullName = $"MinVer CLI {informationalVersion}" };

            app.HelpOption();

            var workDirArg = app.Argument("workingDirectory", "Working directory (optional)");

            var autoIncrementOption = app.Option("-a|--auto-increment <VERSION_PART>", VersionPartExtensions.ValidValues, CommandOptionType.SingleValue);
            var buildMetaOption = app.Option("-b|--build-metadata <BUILD_METADATA>", "", CommandOptionType.SingleValue);
            var defaultPreReleasePhaseOption = app.Option("-d|--default-pre-release-phase <PHASE>", "alpha (default), preview, etc.", CommandOptionType.SingleValue);
            var minMajorMinorOption = app.Option("-m|--minimum-major-minor <MINIMUM_MAJOR_MINOR>", MajorMinor.ValidValues, CommandOptionType.SingleValue);
            var tagPrefixOption = app.Option("-t|--tag-prefix <TAG_PREFIX>", "", CommandOptionType.SingleValue);
            var verbosityOption = app.Option("-v|--verbosity <VERBOSITY>", VerbosityMap.ValidValues, CommandOptionType.SingleValue);
            var showGitHubActionOutput = app.Option("-s|--show-github-action-output", "Show GitHub Action ::set-output variable assignments", CommandOptionType.NoValue);
#if MINVER
            var versionOverrideOption = app.Option("-o|--version-override <VERSION>", "", CommandOptionType.SingleValue);
#endif

            app.OnExecute(() =>
            {
                var workDir = workDirArg.Value ?? ".";

                if (!Directory.Exists(workDir))
                {
                    Logger.ErrorWorkDirDoesNotExist(workDir);
                    return 2;
                }

                if (!Options.TryParse(
                    autoIncrementOption.Value(),
                    buildMetaOption.Value(),
                    defaultPreReleasePhaseOption.Value(),
                    minMajorMinorOption.Value(),
                    tagPrefixOption.Value(),
                    verbosityOption.Value(),
                    showGitHubActionOutput.Values.Count > 0,
#if MINVER
                    versionOverrideOption.Value(),
#endif
                    out var options))
                {
                    return 2;
                }

#if MINVER_CLI
                if (!Options.TryParseEnvVars(out var envOptions))
                {
                    return 2;
                }

                options = options.Mask(envOptions);
#endif

                var log = new Logger(options.Verbosity);

                if (log.IsDebugEnabled)
                {
                    log.Debug($"MinVer {informationalVersion}.");
                }

                if (options.VersionOverride != null)
                {
                    log.Info($"Using version override {options.VersionOverride}.");

                    Console.Out.WriteLine(options.VersionOverride);

                    return 0;
                }

                var version = Versioner.GetVersion(workDir, options.TagPrefix, options.MinMajorMinor, options.BuildMeta, options.AutoIncrement, options.DefaultPreReleasePhase, log);

                if (options.ShowGitHubActionOutput)
                {
                    Console.Out.WriteLine($"::set-output name=MinVerMajor::{version.Major}");
                    Console.Out.WriteLine($"::set-output name=MinVerMinor::{version.Minor}");
                    Console.Out.WriteLine($"::set-output name=MinVerPatch::{version.Patch}");
                    Console.Out.WriteLine($"::set-output name=MinVerPreReleaseIdentifier::{version.PreReleaseIdentifier}");
                    Console.Out.WriteLine($"::set-output name=MinVerBuildMetaData::{version.BuildMetaData}");
                    Console.Out.WriteLine($"::set-output name=MinVerPreHeight::{version.Height}");
                    Console.Out.WriteLine($"::set-output name=MinVerVersion::{version}");
                }

                Console.Out.WriteLine(version);

                return 0;
            });

            return app.Execute(args);
        }
    }
}
