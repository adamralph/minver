namespace MinVer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.Math;

#pragma warning disable CA1036 // Override methods on comparable types
    public class Version : IComparable<Version>
#pragma warning restore CA1036 // Override methods on comparable types
    {
        private readonly int major;
        private readonly int minor;
        private readonly int patch;
        private readonly List<string> preReleaseIdentifiers;
        private readonly int height;
        private readonly string buildMetadata;

        public Version(string defaultPreReleasePhase) : this(default, default, default, new List<string> { defaultPreReleasePhase, "0" }, default, default) { }

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
            if (other == default)
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
            minMajorMinor == default || minMajorMinor.Major < this.major || (minMajorMinor.Major == this.major && minMajorMinor.Minor <= this.minor)
                ? this
                : new Version(minMajorMinor.Major, minMajorMinor.Minor, default, new[] { defaultPreReleasePhase, "0" }, this.height, this.buildMetadata);

        public Version WithHeight(int height, VersionPart autoIncrement, string defaultPreReleasePhase)
        {
            if (this.preReleaseIdentifiers.Count == 0 && height > 0)
            {
                switch (autoIncrement)
                {
                    case VersionPart.Major:
                        return new Version(this.major + 1, 0, 0, new[] { defaultPreReleasePhase, "0" }, height, default);
                    case VersionPart.Minor:
                        return new Version(this.major, this.minor + 1, 0, new[] { defaultPreReleasePhase, "0" }, height, default);
                    case VersionPart.Patch:
                        return new Version(this.major, this.minor, this.patch + 1, new[] { defaultPreReleasePhase, "0" }, height, default);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(autoIncrement));
                }
            }

            return new Version(this.major, this.minor, this.patch, this.preReleaseIdentifiers, height, height == 0 ? this.buildMetadata : default);
        }

        public Version AddBuildMetadata(string buildMetadata)
        {
            var separator = !string.IsNullOrEmpty(this.buildMetadata) && !string.IsNullOrEmpty(buildMetadata) ? "." : "";
            return new Version(this.major, this.minor, this.patch, this.preReleaseIdentifiers, this.height, $"{this.buildMetadata}{separator}{buildMetadata}");
        }

        public static bool TryParse(string text, out Version version) => (version = ParseOrDefault(text, default)) != default;

        public static Version ParseOrDefault(string text, string prefix) =>
            text == default || !text.StartsWith(prefix ?? "") ? default : ParseOrDefault(text.Substring(prefix?.Length ?? 0));

        private static Version ParseOrDefault(string text)
        {
            var versionAndMeta = text.Split(new[] { '+' }, 2);
            var numbersAndPre = versionAndMeta[0].Split(new[] { '-' }, 2);

            return ParseOrDefault(
                numbersAndPre[0].Split('.'),
                numbersAndPre.Length > 1 ? numbersAndPre[1].Split('.') : default,
                versionAndMeta.Length > 1 ? versionAndMeta[1] : default);
        }

        private static Version ParseOrDefault(string[] numbers, IEnumerable<string> pre, string meta) =>
            numbers?.Length == 3 &&
                    int.TryParse(numbers[0], out var major) &&
                    int.TryParse(numbers[1], out var minor) &&
                    int.TryParse(numbers[2], out var patch)
                ? new Version(major, minor, patch, pre, default, meta)
                : default;
    }
}
