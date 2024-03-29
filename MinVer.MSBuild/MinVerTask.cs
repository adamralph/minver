#if NET
using System.Globalization;
#endif
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MinVer.MSBuild;

public class MinVerTask : ToolTask
{
    public string? GitWorkingDirectory { get; set; }

    public string? AutoIncrement { get; set; }

    public string? BuildMetadata { get; set; }

    public string? DefaultPreReleaseIdentifiers { get; set; }

    public string? DefaultPreReleasePhase { get; set; }

    public string? IgnoreHeight { get; set; }

    public string? MinimumMajorMinor { get; set; }

    public string? TagPrefix { get; set; }

    public string? Verbosity { get; set; }

    public string? VersionOverride { get; set; }

    public string TargetFramework { get; set; } = "net6.0";

    [Output]
    public string? Version { get; set; }

    protected override string ToolName => "dotnet";

    protected override string GenerateFullPathToTool() => "dotnet";

    protected override MessageImportance StandardErrorLoggingImportance => this.Verbosity is not null and ("detailed" or "d" or "diagnostic" or "diag") ? MessageImportance.High : MessageImportance.Low;

    protected override bool SkipTaskExecution() => base.SkipTaskExecution();

    protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
    {
        if (singleLine is not null && !singleLine.StartsWith("MinVer", StringComparison.Ordinal))
        {
            this.Version = singleLine;
        }

        base.LogEventsFromTextOutput(singleLine, messageImportance);
    }

    protected override string GenerateCommandLineCommands()
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var minVerPath = Path.GetFullPath(Path.Combine(assemblyDirectory!, "..", "..", "..", "minver", this.TargetFramework, "MinVer.dll"));

        var builder = new StringBuilder();

#if NET
        _ = builder.Append(CultureInfo.InvariantCulture, $"\"{minVerPath}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"\"{this.GitWorkingDirectory}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--auto-increment \"{this.AutoIncrement}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--build-metadata \"{this.BuildMetadata}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--default-pre-release-identifiers \"{this.DefaultPreReleaseIdentifiers}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--default-pre-release-phase \"{this.DefaultPreReleasePhase}\" ");

        if (this.IgnoreHeight?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            _ = builder.Append("--ignore-height ");
        }

        _ = builder.Append(CultureInfo.InvariantCulture, $"--minimum-major-minor \"{this.MinimumMajorMinor}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--tag-prefix \"{this.TagPrefix}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--verbosity \"{this.Verbosity}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--version-override \"{this.VersionOverride}\" ");
#else
        _ = builder.Append($"\"{minVerPath}\" ");
        _ = builder.Append($"\"{this.GitWorkingDirectory}\" ");
        _ = builder.Append($"--auto-increment \"{this.AutoIncrement}\" ");
        _ = builder.Append($"--build-metadata \"{this.BuildMetadata}\" ");
        _ = builder.Append($"--default-pre-release-identifiers \"{this.DefaultPreReleaseIdentifiers}\" ");
        _ = builder.Append($"--default-pre-release-phase \"{this.DefaultPreReleasePhase}\" ");

        if (this.IgnoreHeight?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            _ = builder.Append("--ignore-height ");
        }

        _ = builder.Append($"--minimum-major-minor \"{this.MinimumMajorMinor}\" ");
        _ = builder.Append($"--tag-prefix \"{this.TagPrefix}\" ");
        _ = builder.Append($"--verbosity \"{this.Verbosity}\" ");
        _ = builder.Append($"--version-override \"{this.VersionOverride}\" ");
#endif

        return builder.ToString();
    }
}
