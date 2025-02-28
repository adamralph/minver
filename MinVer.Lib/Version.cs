using NuGet.Versioning;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MinVer.Lib;

public class Version : SemanticVersion
{
    private readonly List<string> preReleaseIdentifiers;
    public int Height { get; private init; }

    public Version(IEnumerable<string> defaultPreReleaseIdentifiers) : this(0, 0, 0, [.. defaultPreReleaseIdentifiers], 0, "") { }

    internal Version(int major, int minor, int patch, List<string> preReleaseIdentifiers, int height, string? buildMetadata) :
        base(
            major,
            minor,
            patch,
            height > 0
                ? preReleaseIdentifiers.Append(height.ToString(CultureInfo.InvariantCulture))
                : preReleaseIdentifiers,
            buildMetadata)
    {
        this.preReleaseIdentifiers = preReleaseIdentifiers;
        this.Height = height;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider) =>
        $"{this.Major}.{this.Minor}.{this.Patch}{(string.IsNullOrEmpty(this.Release) ? "" : $"-{this.Release}")}{(string.IsNullOrEmpty(this.Metadata) ? "" : $"+{this.Metadata}")}";

    public Version Satisfying(MajorMinor minMajorMinor, IEnumerable<string> defaultPreReleaseIdentifiers)
    {
        minMajorMinor = minMajorMinor ?? throw new ArgumentNullException(nameof(minMajorMinor));

        return minMajorMinor.Major < this.Major || (minMajorMinor.Major == this.Major && minMajorMinor.Minor <= this.Minor)
            ? this
            : new Version(minMajorMinor.Major, minMajorMinor.Minor, 0, [.. defaultPreReleaseIdentifiers], this.Height, this.Metadata);
    }

    public Version WithHeight(bool ignoreHeight, int newHeight, VersionPart autoIncrement, IEnumerable<string> defaultPreReleaseIdentifiers)
    {
        if (newHeight == 0)
        {
            // This is an explicitly tagged commit.
            return new Version(this.Major, this.Minor, this.Patch, this.preReleaseIdentifiers, newHeight, this.Metadata);
        }

        var identifiers = this.preReleaseIdentifiers.Count == 0
            ? defaultPreReleaseIdentifiers
            : this.preReleaseIdentifiers;

        return autoIncrement switch
        {
            VersionPart.Major => new Version(this.Major + newHeight, 0, 0, [.. identifiers], ignoreHeight ? 0 : newHeight, ""),
            VersionPart.Minor => new Version(this.Major, this.Minor + newHeight, 0, [.. identifiers], ignoreHeight ? 0 : newHeight, ""),
            VersionPart.Patch => new Version(this.Major, this.Minor, this.Patch + newHeight, [.. identifiers], ignoreHeight ? 0 : newHeight, ""),
            _ => throw new ArgumentOutOfRangeException(nameof(autoIncrement)),
        };
    }

    public Version AddBuildMetadata(string newBuildMetadata)
    {
        var separator = !string.IsNullOrEmpty(this.Metadata) && !string.IsNullOrEmpty(newBuildMetadata) ? "." : "";
        return new Version(this.Major, this.Minor, this.Patch, this.preReleaseIdentifiers, this.Height, $"{this.Metadata}{separator}{newBuildMetadata}");
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
