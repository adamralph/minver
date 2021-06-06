using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace MinVer.Lib
{
    public class Version : IComparable<Version>
    {
        private readonly int major;
        private readonly int minor;
        private readonly int patch;
        private readonly List<string> preReleaseIdentifiers;
        private readonly int height;
        private readonly string buildMetadata;

        public Version(string defaultPreReleasePhase) : this(0, 0, 0, new List<string> { defaultPreReleasePhase, "0" }, 0, null) { }

        private Version(int major, int minor, int patch, IEnumerable<string> preReleaseIdentifiers, int height, string buildMetadata)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.preReleaseIdentifiers = preReleaseIdentifiers?.ToList() ?? new List<string>();
            this.height = height;
            this.buildMetadata = buildMetadata;
        }

        public override string ToString() =>
            $"{this.major}.{this.minor}.{this.patch}{(this.preReleaseIdentifiers.Count == 0 ? "" : $"-{string.Join(".", this.preReleaseIdentifiers)}")}{(this.height == 0 ? "" : $".{this.height}")}{(string.IsNullOrEmpty(this.buildMetadata) ? "" : $"+{this.buildMetadata}")}";

        public int CompareTo(Version other)
        {
            if (other == null)
            {
                return 1;
            }

            var major = this.major.CompareTo(other.major);
            if (major != 0)
            {
                return major;
            }

            var minor = this.minor.CompareTo(other.minor);
            if (minor != 0)
            {
                return minor;
            }

            var patch = this.patch.CompareTo(other.patch);
            if (patch != 0)
            {
                return patch;
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

        public Version Satisfying(MajorMinor minMajorMinor, string defaultPreReleasePhase) =>
            minMajorMinor == null || minMajorMinor.Major < this.major || (minMajorMinor.Major == this.major && minMajorMinor.Minor <= this.minor)
                ? this
                : new Version(minMajorMinor.Major, minMajorMinor.Minor, 0, new[] { defaultPreReleasePhase, "0" }, this.height, this.buildMetadata);

        public Version WithHeight(int height, VersionPart autoIncrement, string defaultPreReleasePhase)
        {
            if (this.preReleaseIdentifiers.Count == 0 && height > 0)
            {
                switch (autoIncrement)
                {
                    case VersionPart.Major:
                        return new Version(this.major + 1, 0, 0, new[] { defaultPreReleasePhase, "0" }, height, null);
                    case VersionPart.Minor:
                        return new Version(this.major, this.minor + 1, 0, new[] { defaultPreReleasePhase, "0" }, height, null);
                    case VersionPart.Patch:
                        return new Version(this.major, this.minor, this.patch + 1, new[] { defaultPreReleasePhase, "0" }, height, null);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(autoIncrement));
                }
            }

            return new Version(this.major, this.minor, this.patch, this.preReleaseIdentifiers, height, height == 0 ? this.buildMetadata : null);
        }

        public Version AddBuildMetadata(string buildMetadata)
        {
            var separator = !string.IsNullOrEmpty(this.buildMetadata) && !string.IsNullOrEmpty(buildMetadata) ? "." : "";
            return new Version(this.major, this.minor, this.patch, this.preReleaseIdentifiers, this.height, $"{this.buildMetadata}{separator}{buildMetadata}");
        }

        public static bool TryParse(string text, out Version version) => (version = ParseOrDefault(text, null)) != null;

        public static Version ParseOrDefault(string text, string prefix) =>
            text == null || !text.StartsWith(prefix ?? "", StringComparison.OrdinalIgnoreCase) ? null : ParseOrDefault(text.Substring(prefix?.Length ?? 0));

        private static Version ParseOrDefault(string text)
        {
            var versionAndMeta = text.Split(new[] { '+' }, 2);
            var numbersAndPre = versionAndMeta[0].Split(new[] { '-' }, 2);

            return ParseOrDefault(
                numbersAndPre[0].Split('.'),
                numbersAndPre.Length > 1 ? numbersAndPre[1].Split('.') : null,
                versionAndMeta.Length > 1 ? versionAndMeta[1] : null);
        }

        private static Version ParseOrDefault(string[] numbers, IEnumerable<string> pre, string meta) =>
            numbers?.Length == 3 &&
                    int.TryParse(numbers[0], out var major) &&
                    int.TryParse(numbers[1], out var minor) &&
                    int.TryParse(numbers[2], out var patch)
                ? new Version(major, minor, patch, pre, 0, meta)
                : null;

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) ||
            (!(obj is null) && obj is Version && this.CompareTo(obj as Version) == 0);

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
                code = (code * 23) + this.buildMetadata.GetHashCode();

                return code;
            }
        }

        public static bool operator ==(Version left, Version right) => left is null ? right is null : left.Equals(right);

        public static bool operator !=(Version left, Version right) => !(left == right);

        public static bool operator <(Version left, Version right) => left is null ? right is object : left.CompareTo(right) < 0;

        public static bool operator <=(Version left, Version right) => left is null || left.CompareTo(right) <= 0;

        public static bool operator >(Version left, Version right) => left is object && left.CompareTo(right) > 0;

        public static bool operator >=(Version left, Version right) => left is null ? right is null : left.CompareTo(right) >= 0;
    }
}
