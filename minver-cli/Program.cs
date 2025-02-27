using McMaster.Extensions.CommandLineUtils;
using MinVer;
using MinVer.Lib;
using System.Reflection;
using Version = MinVer.Lib.Version;

var informationalVersion = typeof(Versioner).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;

using var app = new CommandLineApplication();
app.Name = "minver";
app.FullName = $"MinVer CLI {informationalVersion}";

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
var ignorePreReleaseIdentifiersOption = app.Option<bool>("-o|--ignore-pre-release-identifiers", "Ignore pre-release identifiers", CommandOptionType.NoValue);
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
        ignorePreReleaseIdentifiersOption.HasValue() ? true : null,
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
        log.Warn(1008, "MinVerDefaultPreReleasePhase is deprecated and will be removed in the next major version. Use MinVerDefaultPreReleaseIdentifiers instead, with an additional \"0\" identifier. For example, if you are setting MinVerDefaultPreReleasePhase to \"preview\", set MinVerDefaultPreReleaseIdentifiers to \"preview.0\" instead.");

        defaultPreReleaseIdentifiers ??= [options.DefaultPreReleasePhase, "0",];
    }

    // If we were told to ignore pre-release identifiers, don't add the default pre-release identifier
    if (options.IgnorePreReleaseIdentifiers != null && options.IgnorePreReleaseIdentifiers.Value)
    {
        if (defaultPreReleaseIdentifiers == null)
        {
            // We were told to ignore pre-release identifiers - make sure they're cleared
            defaultPreReleaseIdentifiers = [];
        }
        else
        {
            // Warn the user that the default pre-release identifiers are being ignored
            // due to the conflict with the ignore pre-release identifiers option
            log.Warn(1009, MinVerError.IgnorePreReleaseIdentifiersAndDefaultPreReleaseIdentifiersConflict);
        }
    }
    else
    {
        // We were not told to ignore pre-release identifiers, but were not provided with a string to use
        // Fallback to using the default pre-release identifiers
        defaultPreReleaseIdentifiers ??= PreReleaseIdentifiers.Default;
    }

    Version version;
    try
    {
        version = Versioner.GetVersion(workDir, options.TagPrefix ?? "", options.MinMajorMinor ?? MajorMinor.Default, options.BuildMeta ?? "", options.AutoIncrement ?? default, defaultPreReleaseIdentifiers, options.IgnoreHeight ?? false, log);
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
