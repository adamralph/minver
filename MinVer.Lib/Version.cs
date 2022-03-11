using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static System.Math;

namespace MinVer.Lib;

public class Version : IComparable<Version>
{
    private readonly int major;
    private readonly int minor;
    private readonly int patch;
    private readonly List<string> preReleaseIdentifiers;
    private readonly int height;
    private readonly string buildMetadata;

    public Version(string defaultPreReleasePhase) : this(0, 0, 0, new List<string> { defaultPreReleasePhase, "0", }, 0, "") { }

    private Version(int major, int minor, int patch, List<string> preReleaseIdentifiers, int height, string buildMetadata)
    {
        this.major = major;
        this.minor = minor;
        this.patch = patch;
        this.preReleaseIdentifiers = preReleaseIdentifiers;
        this.height = height;
        this.buildMetadata = buildMetadata;
    }

    public override string ToString() =>
        $"{this.major}.{this.minor}.{this.patch}{(this.preReleaseIdentifiers.Count == 0 ? "" : $"-{string.Join(".", this.preReleaseIdentifiers)}")}{(this.height == 0 ? "" : $".{this.height}")}{(string.IsNullOrEmpty(this.buildMetadata) ? "" : $"+{this.buildMetadata}")}";

    public int CompareTo(Version? other)
    {
        if (other == null)
        {
            return 1;
        }

        var majorComparison = this.major.CompareTo(other.major);
        if (majorComparison != 0)
        {
            return majorComparison;
        }

        var minorComparison = this.minor.CompareTo(other.minor);
        if (minorComparison != 0)
        {
            return minorComparison;
        }

        var patchComparison = this.patch.CompareTo(other.patch);
        if (patchComparison != 0)
        {
            return patchComparison;
        }

        if (this.preReleaseIdentifiers.Count > 0 && other.preReleaseIdentifiers.Count == 0)
        {
            return -1;
        }

        if (this.preReleaseIdentifiers.Count == 0 && other.preReleaseIdentifiers.Count > 0)
        {
            return 1;
        }

        var maxCount = Max(this.preReleaseIdentifiers.Count, other.preReleaseIdentifiers.Count);
        for (var index = 0; index < maxCount; ++index)
        {
            if (this.preReleaseIdentifiers.Count == index && other.preReleaseIdentifiers.Count > index)
            {
                return -1;
            }

            if (this.preReleaseIdentifiers.Count > index && other.preReleaseIdentifiers.Count == index)
            {
                return 1;
            }

            if (int.TryParse(this.preReleaseIdentifiers[index], out var thisNumber) && int.TryParse(other.preReleaseIdentifiers[index], out var otherNumber))
            {
                var number = thisNumber.CompareTo(otherNumber);
                if (number != 0)
                {
                    return number;
                }
            }
            else
            {
                var text = string.CompareOrdinal(this.preReleaseIdentifiers[index], other.preReleaseIdentifiers[index]);
                if (text != 0)
                {
                    return text;
                }
            }
        }

        return this.height.CompareTo(other.height);
    }

    public Version Satisfying(MajorMinor minMajorMinor, string defaultPreReleasePhase)
    {
        minMajorMinor = minMajorMinor ?? throw new ArgumentNullException(nameof(minMajorMinor));

        return minMajorMinor.Major < this.major || (minMajorMinor.Major == this.major && minMajorMinor.Minor <= this.minor)
            ? this
            : new Version(minMajorMinor.Major, minMajorMinor.Minor, 0, new List<string> { defaultPreReleasePhase, "0", }, this.height, this.buildMetadata);
    }

    public Version WithHeight(int newHeight, VersionPart autoIncrement, string defaultPreReleasePhase) =>
        this.preReleaseIdentifiers.Count == 0 && newHeight > 0
            ? autoIncrement switch
            {
                VersionPart.Major => new Version(this.major + 1, 0, 0, new List<string> { defaultPreReleasePhase, "0", }, newHeight, ""),
                VersionPart.Minor => new Version(this.major, this.minor + 1, 0, new List<string> { defaultPreReleasePhase, "0", }, newHeight, ""),
                VersionPart.Patch => new Version(this.major, this.minor, this.patch + 1, new List<string> { defaultPreReleasePhase, "0", }, newHeight, ""),
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

        var versionAndMeta = text[prefix.Length..].Split(new[] { '+', }, 2);
        var numbersAndPre = versionAndMeta[0].Split(new[] { '-', }, 2);
        var numbers = numbersAndPre[0].Split('.');

        if (numbers.Length != 3 ||
            !int.TryParse(numbers[0], out var major) ||
            !int.TryParse(numbers[1], out var minor) ||
            !int.TryParse(numbers[2], out var patch))
        {
            return false;
        }

        var pre = numbersAndPre.Length > 1 ? numbersAndPre[1].Split('.').ToList() : new List<string>();
        var meta = versionAndMeta.Length > 1 ? versionAndMeta[1] : "";

        version = new Version(major, minor, patch, pre, 0, meta);

        return true;
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) ||
        (obj is Version version && this.CompareTo(version) == 0);

    public override int GetHashCode()
    {
        unchecked
        {
            var code = 17;

            code = (code * 23) + this.major.GetHashCode();
            code = (code * 23) + this.minor.GetHashCode();
            code = (code * 23) + this.patch.GetHashCode();
            code = (code * 23) + this.preReleaseIdentifiers.GetHashCode();
            code = (code * 23) + this.height.GetHashCode();
            code = (code * 23) + this.buildMetadata.GetHashCode(StringComparison.Ordinal);

            return code;
        }
    }

    public static bool operator ==(Version? left, Version? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(Version? left, Version? right) => !(left == right);

    public static bool operator <(Version? left, Version? right) => left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(Version? left, Version? right) => left is null || left.CompareTo(right) <= 0;

    public static bool operator >(Version? left, Version? right) => left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(Version? left, Version? right) => left is null ? right is null : left.CompareTo(right) >= 0;
}
