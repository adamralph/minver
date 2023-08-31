using System;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using MinVer;
using MinVer.Lib;
using Version = MinVer.Lib.Version;

var informationalVersion = typeof(Versioner).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;

using var app = new CommandLineApplication { Name = "minver", FullName = $"MinVer CLI {informationalVersion}", };

app.HelpOption();

var workDirArg = app.Argument("workingDirectory", "Working directory (optional)");

var autoIncrementOption = app.Option("-a|--auto-increment <VERSION_PART>", VersionPartExtensions.ValidValues, CommandOptionType.SingleValue);
var buildMetaOption = app.Option("-b|--build-metadata <BUILD_METADATA>", "", CommandOptionType.SingleValue);
var defaultPreReleaseIdentifiersOption = app.Option("-p|--default-pre-release-identifiers <IDENTIFIERS>", "alpha.0 (default), preview.0, etc.", CommandOptionType.SingleValue);
var defaultPreReleasePhaseOption = app.Option("-d|--default-pre-release-phase <PHASE>", "alpha (default), preview, etc.", CommandOptionType.SingleValue);
var ignoreHeightOption = app.Option<bool>("-i|--ignore-height", "Use the latest tag (or root commit) as-is, without adding height", CommandOptionType.NoValue);
var minMajorMinorOption = app.Option("-m|--minimum-major-minor <MINIMUM_MAJOR_MINOR>", MajorMinor.ValidValues, CommandOptionType.SingleValue);
var tagPrefixOption = app.Option("-t|--tag-prefix <TAG_PREFIX>", "", CommandOptionType.SingleValue);
var verbosityOption = app.Option("-v|--verbosity <VERBOSITY>", VerbosityMap.ValidValues, CommandOptionType.SingleValue);
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
        defaultPreReleaseIdentifiersOption.Value(),
        defaultPreReleasePhaseOption.Value(),
        ignoreHeightOption.HasValue() ? true : null,
        minMajorMinorOption.Value(),
        tagPrefixOption.Value(),
        verbosityOption.Value(),
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

    var log = new Logger(options.Verbosity ?? default);

    _ = log.IsDebugEnabled && log.Debug($"MinVer {informationalVersion}.");

    if (options.VersionOverride != null)
    {
        _ = log.IsInfoEnabled && log.Info($"Using version override {options.VersionOverride}.");

        Console.Out.WriteLine(options.VersionOverride);

        return 0;
    }

    var defaultPreReleaseIdentifiers = options.DefaultPreReleaseIdentifiers;
    if (!string.IsNullOrEmpty(options.DefaultPreReleasePhase))
    {
        log.Warn(1008, $"MinVerDefaultPreReleasePhase is deprecated and will be removed in the next major version. Use MinVerDefaultPreReleaseIdentifiers instead, with an additional \"0\" identifier. For example, if you are setting MinVerDefaultPreReleasePhase to \"preview\", set MinVerDefaultPreReleaseIdentifiers to \"preview.0\" instead.");

        defaultPreReleaseIdentifiers ??= new[] { options.DefaultPreReleasePhase, "0", };
    }

    Version version;
    try
    {
        version = Versioner.GetVersion(workDir, options.TagPrefix ?? "", options.MinMajorMinor ?? MajorMinor.Default, options.BuildMeta ?? "", options.AutoIncrement ?? default, defaultPreReleaseIdentifiers ?? PreReleaseIdentifiers.Default, options.IgnoreHeight ?? false, log);
    }
    catch (NoGitException ex)
    {
        Logger.ErrorNoGit(ex.Message);
        return 2;
    }

    Console.Out.WriteLine(version);

    return 0;
});

return app.Execute(args);
