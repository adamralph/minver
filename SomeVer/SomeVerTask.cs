using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MinVer.Lib;

namespace SomeVer;

public class SomeVerTask : Microsoft.Build.Utilities.Task
{
    public string? AutoIncrement { get; set; }

    public string? BuildMetadata { get; set; }

    public string[]? DefaultPreReleaseIdentifiers { get; set; }

    public string? IgnoreHeight { get; set; }

    public string? IncludeDefaultPreReleaseIdentifiersWithPrereleases { get; set; }

    public string? MinimumMajorMinor { get; set; }

    public string? TagPrefix { get; set; }

    [Required]
    public string? WorkingDirectory { get; set; }

    [Output]
    public string Version { get; private set; } = string.Empty;

    [Output]
    public int Major { get; private set; } = 0;

    [Output]
    public int Minor { get; private set; } = 0;

    [Output]
    public int Patch { get; private set; } = 0;

    [Output]
    public string Label { get; private set; } = string.Empty;

    [Output]
    public int Height { get; private set; } = 0;

    [Output]
    public string Sha { get; private set; } = string.Empty;

    [Output]
    public string ShortSha { get; private set; } = string.Empty;

    public override bool Execute()
    {
        var log = new MSBuildMinVerLogger(this.Log);

        this.WorkingDirectory ??= Environment.CurrentDirectory;

        try
        {
            var data = Versioner.GetVersionData(
                this.WorkingDirectory,
                this.TagPrefix ?? string.Empty,
                MajorMinor.TryParse(this.MinimumMajorMinor, out var minMajorMinor) ? minMajorMinor : MajorMinor.Default,
                this.BuildMetadata ?? string.Empty,
                Enum.TryParse<VersionPart>(this.AutoIncrement, ignoreCase: true, out var autoIncrement) ? autoIncrement : default,
                this.DefaultPreReleaseIdentifiers ?? PreReleaseIdentifiers.Default,
                bool.TryParse(this.IgnoreHeight, out var ignoreHeight) && ignoreHeight,
                bool.TryParse(this.IncludeDefaultPreReleaseIdentifiersWithPrereleases, out var includeDefaultPreReleaseIdentifiersWithPrereleases) && includeDefaultPreReleaseIdentifiersWithPrereleases,
                log);

            this.Version = data.Version;
            this.Major = data.Major;
            this.Minor = data.Minor;
            this.Minor = data.Patch;
            this.Patch = data.Patch;
            this.Label = data.Label;
            this.Sha = data.Sha;
            this.ShortSha = data.ShortSha;
            this.Height = data.Height;

            return true;
        }
        catch (NoGitException ex)
        {
            this.Log.LogErrorFromException(ex);
            return false;
        }
    }

    private sealed class MSBuildMinVerLogger(TaskLoggingHelper log) : MinVer.Lib.ILogger
    {
        public bool IsTraceEnabled { get; }

        public bool IsDebugEnabled { get; }

        public bool IsInfoEnabled { get; } = true;

        public bool IsWarnEnabled { get; } = true;

        public bool Debug(string message)
        {
            log.LogMessage(MessageImportance.Low, message);
            return true;
        }

        public bool Info(string message)
        {
            log.LogMessage(MessageImportance.Low, message);
            return true;
        }


        public bool Trace(string message)
        {
            log.LogMessage(MessageImportance.Low, message);
            return true;
        }

        public bool Warn(int code, string message)
        {
            log.LogWarning(message);
            return true;
        }
    }
}
