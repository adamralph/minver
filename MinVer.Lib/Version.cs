using System.Diagnostics.CodeAnalysis;

namespace MinVer.Lib;

public class Version
{
    private readonly int major;
    private readonly int minor;
    private readonly int patch;
    private readonly List<string> preReleaseIdentifiers;
    private readonly int height;
    private readonly string? buildMetadata;

    public Version(IEnumerable<string> defaultPreReleaseIdentifiers) : this(0, 0, 0, [.. defaultPreReleaseIdentifiers], 0, "") { }

    private Version(int major, int minor, int patch, List<string> preReleaseIdentifiers, int height, string? buildMetadata)
    {
        this.major = major;
        this.minor = minor;
        this.patch = patch;
        this.preReleaseIdentifiers = preReleaseIdentifiers;
        this.height = height;
        this.buildMetadata = buildMetadata;
    }

    public override string ToString() =>
        $"{this.major}.{this.minor}.{this.patch}{(string.IsNullOrEmpty(this.Release) ? "" : $"-{this.Release}")}{(string.IsNullOrEmpty(this.buildMetadata) ? "" : $"+{this.buildMetadata}")}";

    public Version Satisfying(MajorMinor minMajorMinor, IEnumerable<string> defaultPreReleaseIdentifiers)
    {
        minMajorMinor = minMajorMinor ?? throw new ArgumentNullException(nameof(minMajorMinor));

        return minMajorMinor.Major < this.major || (minMajorMinor.Major == this.major && minMajorMinor.Minor <= this.minor)
            ? this
            : new Version(minMajorMinor.Major, minMajorMinor.Minor, 0, [.. defaultPreReleaseIdentifiers], this.height, this.buildMetadata);
    }

    public Version WithHeight(int newHeight, VersionPart autoIncrement, IEnumerable<string> defaultPreReleaseIdentifiers) =>
        this.preReleaseIdentifiers.Count == 0 && newHeight > 0
            ? autoIncrement switch
            {
                VersionPart.Major => new Version(this.major + 1, 0, 0, [.. defaultPreReleaseIdentifiers], newHeight, ""),
                VersionPart.Minor => new Version(this.major, this.minor + 1, 0, [.. defaultPreReleaseIdentifiers], newHeight, ""),
                VersionPart.Patch => new Version(this.major, this.minor, this.patch + 1, [.. defaultPreReleaseIdentifiers], newHeight, ""),
                _ => throw new ArgumentOutOfRangeException(nameof(autoIncrement)),
            }
            : new Version(this.major, this.minor, this.patch, this.preReleaseIdentifiers, newHeight, newHeight == 0 ? this.buildMetadata : "");

    public Version AddBuildMetadata(string newBuildMetadata)
    {
        var separator = !string.IsNullOrEmpty(this.buildMetadata) && !string.IsNullOrEmpty(newBuildMetadata) ? "." : "";
        return new Version(this.major, this.minor, this.patch, this.preReleaseIdentifiers, this.height, $"{this.buildMetadata}{separator}{newBuildMetadata}");
    }

    public static bool TryParse(string text, [NotNullWhen(returnValue: true)] out Version? version, string prefix = "")
    {
        text = text ?? throw new ArgumentNullException(nameof(text));
        prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));

        version = null;

        if (!text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!SemanticVersion.TryParse(text[prefix.Length..], out var semanticVersion))
        {
            return false;
        }

        version = new Version(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, [.. semanticVersion.ReleaseLabels], 0, semanticVersion.Metadata);
        return true;
    }
}
