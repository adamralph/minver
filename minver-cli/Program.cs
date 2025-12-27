using System.CommandLine;
using System.Reflection;
using MinVer;
using MinVer.Lib;
using Version = MinVer.Lib.Version;

Argument<string> workDirArg = new("workingDirectory") { Description = "Working directory", DefaultValueFactory = _ => ".", };
workDirArg.AcceptLegalFilePathsOnly();

Option<string?> autoIncrementOption = new("--auto-increment", "-a") { Description = $"{VersionPartExtensions.ValidValues}", };
Option<string?> buildMetaOption = new("--build-metadata", "-b");
Option<string?> defaultPreReleaseIdentifiersOption = new("--default-pre-release-identifiers", "-p") { Description = "alpha.0 (default), preview.0, etc.", };
Option<string?> defaultPreReleasePhaseOption = new("--default-pre-release-phase", "-d") { Description = "alpha (default), preview, etc.", };
Option<bool?> ignoreHeightOption = new("--ignore-height", "-i") { Description = "Use the latest tag (or root commit) as-is, without adding height", };
Option<string?> minMajorMinorOption = new("--minimum-major-minor", "-m") { Description = MajorMinor.ValidValues, };
Option<string?> tagPrefixOption = new("--tag-prefix", "-t");
Option<string?> verbosityOption = new("--verbosity", "-v") { Description = VerbosityMap.ValidValues, };
#if MINVER
Option<string?> versionOverrideOption = new("--version-override", "-o");
#endif

var informationalVersion = typeof(Versioner).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;
RootCommand cmd = new($"MinVer CLI {informationalVersion}")
{
    workDirArg,
    autoIncrementOption,
    buildMetaOption,
    defaultPreReleaseIdentifiersOption,
    defaultPreReleasePhaseOption,
    ignoreHeightOption,
    minMajorMinorOption,
    tagPrefixOption,
    verbosityOption,
#if MINVER
    versionOverrideOption,
#endif
};

cmd.SetAction(async cmdLine =>
{
    var workDir = cmdLine.GetValue(workDirArg)!;

    if (!Directory.Exists(workDir))
    {
        Logger.ErrorWorkDirDoesNotExist(workDir);
        return 2;
    }

    if (!Options.TryParse(
            cmdLine.GetValue(autoIncrementOption),
            cmdLine.GetValue(buildMetaOption),
            cmdLine.GetValue(defaultPreReleaseIdentifiersOption),
            cmdLine.GetValue(defaultPreReleasePhaseOption),
            cmdLine.GetValue(ignoreHeightOption),
            cmdLine.GetValue(minMajorMinorOption),
            cmdLine.GetValue(tagPrefixOption),
            cmdLine.GetValue(verbosityOption),
#if MINVER
            cmdLine.GetValue(versionOverrideOption),
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

        await Console.Out.WriteLineAsync(options.VersionOverride.ToString());

        return 0;
    }

    var defaultPreReleaseIdentifiers = options.DefaultPreReleaseIdentifiers;
    if (!string.IsNullOrEmpty(options.DefaultPreReleasePhase))
    {
        log.Warn(1008, "MinVerDefaultPreReleasePhase is deprecated and will be removed in the next major version. Use MinVerDefaultPreReleaseIdentifiers instead, with an additional \"0\" identifier. For example, if you are setting MinVerDefaultPreReleasePhase to \"preview\", set MinVerDefaultPreReleaseIdentifiers to \"preview.0\" instead.");

        defaultPreReleaseIdentifiers ??= [options.DefaultPreReleasePhase, "0",];
    }

    Version version;
    try
    {
        version = await Versioner.GetVersion(workDir, options.TagPrefix ?? "", options.MinMajorMinor ?? MajorMinor.Default, options.BuildMeta ?? "", options.AutoIncrement ?? default, defaultPreReleaseIdentifiers ?? PreReleaseIdentifiers.Default, options.IgnoreHeight ?? false, log);
    }
    catch (NoGitException ex)
    {
        Logger.ErrorNoGit(ex.Message);
        return 2;
    }

    await Console.Out.WriteLineAsync(version.ToString());

    return 0;
});

return await cmd.Parse(args).InvokeAsync();
