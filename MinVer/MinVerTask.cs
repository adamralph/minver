using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MinVer.Lib;

using Version = MinVer.Lib.Version;

namespace MinVer;

public class MinVerTask : Task
{
    public string? WorkingDirectory { get; set; }

    public string? AutoIncrement { get; set; }

    public string? BuildMeta { get; set; }

    public string? DefaultPreReleaseIdentifiers { get; set; }

    public string? DefaultPreReleasePhase { get; set; }

    public bool? IgnoreHeight { get; set; }

    public string? MinMajorMinor { get; set; }

    public string? TagPrefix { get; set; }

    public string? Verbosity { get; set; }

    public string? VersionOverride { get; set; }

    [Output]
    public string? Version { get; set; }

    public override bool Execute()
    {
        var log = new Logger(MinVer.Verbosity.Detailed, Log);

        var informationalVersion = typeof(Versioner).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;

        var workDir = WorkingDirectory ?? ".";

        if (!Directory.Exists(workDir))
        {
            log.ErrorWorkDirDoesNotExist(workDir);
            return false;
        }

        if (!Options.TryParse(
            log,
            AutoIncrement,
            BuildMeta,
            DefaultPreReleaseIdentifiers,
            DefaultPreReleasePhase,
            IgnoreHeight,
            MinMajorMinor,
            TagPrefix,
            Verbosity,
            VersionOverride,
            out var options))
        {
            return false;
        }

        _ = log.IsDebugEnabled && log.Debug($"MinVer {informationalVersion}.");

        if (options.VersionOverride != null)
        {
            _ = log.IsInfoEnabled && log.Info($"Using version override {options.VersionOverride}.");

            Version = VersionOverride.ToString();

            return true;
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
            log.ErrorNoGit(ex.Message);
            return false;
        }

        Version = version.ToString();

        return true;
    }
}
