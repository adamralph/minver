using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NuGet.Versioning;

namespace MinVer.Lib;

public class Version : SemanticVersion
{
    private readonly List<string> _preReleaseIdentifiers;
    private readonly int _height;

    public Version(IEnumerable<string> defaultPreReleaseIdentifiers) : this(0, 0, 0, [.. defaultPreReleaseIdentifiers,], 0, "") { }

    private Version(int major, int minor, int patch, List<string> preReleaseIdentifiers, int height, string? buildMetadata) :
        base(
            major,
            minor,
            patch,
            height > 0
                ? preReleaseIdentifiers.Append(height.ToString(CultureInfo.InvariantCulture))
                : preReleaseIdentifiers,
            buildMetadata)
    {
        _preReleaseIdentifiers = preReleaseIdentifiers;
        _height = height;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider) =>
        $"{Major}.{Minor}.{Patch}{(string.IsNullOrEmpty(Release) ? "" : $"-{Release}")}{(string.IsNullOrEmpty(Metadata) ? "" : $"+{Metadata}")}";

    public Version Satisfying(MajorMinor minMajorMinor, IEnumerable<string> defaultPreReleaseIdentifiers)
    {
        minMajorMinor = minMajorMinor ?? throw new ArgumentNullException(nameof(minMajorMinor));

        return minMajorMinor.Major < Major || (minMajorMinor.Major == Major && minMajorMinor.Minor <= Minor)
            ? this
            : new Version(minMajorMinor.Major, minMajorMinor.Minor, 0, [.. defaultPreReleaseIdentifiers,], _height, Metadata);
    }

    public Version WithHeight(int newHeight, VersionPart autoIncrement, IEnumerable<string> defaultPreReleaseIdentifiers) =>
        _preReleaseIdentifiers.Count == 0 && newHeight > 0
            ? autoIncrement switch
            {
                VersionPart.Major => new Version(Major + 1, 0, 0, [.. defaultPreReleaseIdentifiers,], newHeight, ""),
                VersionPart.Minor => new Version(Major, Minor + 1, 0, [.. defaultPreReleaseIdentifiers,], newHeight, ""),
                VersionPart.Patch => new Version(Major, Minor, Patch + 1, [.. defaultPreReleaseIdentifiers,], newHeight, ""),
                _ => throw new ArgumentOutOfRangeException(nameof(autoIncrement)),
            }
            : new Version(Major, Minor, Patch, _preReleaseIdentifiers, newHeight, newHeight == 0 ? Metadata : "");

    public Version AddBuildMetadata(string newBuildMetadata)
    {
        var separator = !string.IsNullOrEmpty(Metadata) && !string.IsNullOrEmpty(newBuildMetadata) ? "." : "";
        return new Version(Major, Minor, Patch, _preReleaseIdentifiers, _height, $"{Metadata}{separator}{newBuildMetadata}");
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

        version = new Version(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, [.. semanticVersion.ReleaseLabels,], 0, semanticVersion.Metadata);
        return true;
    }
}
