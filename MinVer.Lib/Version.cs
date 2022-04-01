using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using NuGet.Versioning;

namespace MinVer.Lib;

public class Version : SemanticVersion
{
    private readonly List<string> preReleaseIdentifiers;
    private readonly int height;

    public Version(string defaultPreReleasePhase) : this(0, 0, 0, new List<string> { defaultPreReleasePhase, "0", }, 0, "") { }

    private Version(int major, int minor, int patch, List<string> preReleaseIdentifiers, int height, string buildMetadata) :
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
        this.height = height;
    }

    public override string ToString(string format, IFormatProvider formatProvider) =>
        $"{this.Major}.{this.Minor}.{this.Patch}{(string.IsNullOrEmpty(this.Release) ? "" : $"-{this.Release}")}{(string.IsNullOrEmpty(this.Metadata) ? "" : $"+{this.Metadata}")}";

    public Version Satisfying(MajorMinor minMajorMinor, string defaultPreReleasePhase)
    {
        minMajorMinor = minMajorMinor ?? throw new ArgumentNullException(nameof(minMajorMinor));

        return minMajorMinor.Major < this.Major || (minMajorMinor.Major == this.Major && minMajorMinor.Minor <= this.Minor)
            ? this
            : new Version(minMajorMinor.Major, minMajorMinor.Minor, 0, new List<string> { defaultPreReleasePhase, "0", }, this.height, this.Metadata);
    }

    public Version WithHeight(int newHeight, VersionPart autoIncrement, string defaultPreReleasePhase) =>
        this.preReleaseIdentifiers.Count == 0 && newHeight > 0
            ? autoIncrement switch
            {
                VersionPart.Major => new Version(this.Major + 1, 0, 0, new List<string> { defaultPreReleasePhase, "0", }, newHeight, ""),
                VersionPart.Minor => new Version(this.Major, this.Minor + 1, 0, new List<string> { defaultPreReleasePhase, "0", }, newHeight, ""),
                VersionPart.Patch => new Version(this.Major, this.Minor, this.Patch + 1, new List<string> { defaultPreReleasePhase, "0", }, newHeight, ""),
                _ => throw new ArgumentOutOfRangeException(nameof(autoIncrement)),
            }
            : new Version(this.Major, this.Minor, this.Patch, this.preReleaseIdentifiers, newHeight, newHeight == 0 ? this.Metadata : "");

    public Version AddBuildMetadata(string newBuildMetadata)
    {
        var separator = !string.IsNullOrEmpty(this.Metadata) && !string.IsNullOrEmpty(newBuildMetadata) ? "." : "";
        return new Version(this.Major, this.Minor, this.Patch, this.preReleaseIdentifiers, this.height, $"{this.Metadata}{separator}{newBuildMetadata}");
    }

    public static bool TryParse(string text, bool ignoreLeadingZeros, [NotNullWhen(returnValue: true)] out Version? version, string prefix = "")
    {
        text = text ?? throw new ArgumentNullException(nameof(text));
        prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));

        version = null;

        if (!text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (ignoreLeadingZeros)
        {
            var zerosRemoved = new StringBuilder();

            var plusSplit = text.Split('+', 2);
            var dashSplit = plusSplit[0].Split('-', 2);

            _ = zerosRemoved.Append(RemoveLeadingZeros(dashSplit[0]));

            if (dashSplit.Length == 2)
            {
                _ = zerosRemoved.Append('-').Append(RemoveLeadingZeros(dashSplit[1]));
            }

            if (plusSplit.Length == 2)
            {
                _ = zerosRemoved.Append('+').Append(plusSplit[1]);
            }

            text = zerosRemoved.ToString();
        }

        if (!SemanticVersion.TryParse(text[prefix.Length..], out var semanticVersion))
        {
            return false;
        }

        version = new Version(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, semanticVersion.ReleaseLabels.ToList(), 0, semanticVersion.Metadata);
        return true;
    }

    private static string RemoveLeadingZeros(string text) =>
        string.Join('.', text.Split('.').Select(token => token.All(char.IsDigit) ? token.TrimStart('0') : token));
}
