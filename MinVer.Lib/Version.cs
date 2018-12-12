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
        private readonly List<string> preReleaseIdentifiers;
        private readonly int height;
        private readonly string buildMetadata;

        public Version(IReadOnlyCollection<string> preReleaseIdentifiers) : this(default, default, preReleaseIdentifiers) { }

        public Version(int major, int minor, IReadOnlyCollection<string> preReleaseIdentifiers) : this(major, minor, default, preReleaseIdentifiers, default, default) { }

        public Version(int major, int minor, IReadOnlyCollection<string> preReleaseIdentifiers, string buildMetadata) : this(major, minor, default, preReleaseIdentifiers, default, buildMetadata) { }

        private Version(int major, int minor, int patch, IReadOnlyCollection<string> preReleaseIdentifiers, int height, string buildMetadata)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.preReleaseIdentifiers = preReleaseIdentifiers?.ToList() ?? new List<string>();
            this.height = height;
            this.buildMetadata = buildMetadata;
        }

        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public override string ToString() =>
            $"{this.Major}.{this.Minor}.{this.Patch}{(this.preReleaseIdentifiers.Count == 0 ? "" : $"-{string.Join(".", this.preReleaseIdentifiers)}")}{(this.height == 0 ? "" : $".{this.height}")}{(string.IsNullOrEmpty(this.buildMetadata) ? "" : $"+{this.buildMetadata}")}";

        public int CompareTo(Version other)
        {
            if (other == default)
            {
                return 1;
            }

            var major = this.Major.CompareTo(other.Major);
            if (major != 0)
            {
                return major;
            }

            var minor = this.Minor.CompareTo(other.Minor);
            if (minor != 0)
            {
                return minor;
            }

            var patch = this.Patch.CompareTo(other.Patch);
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

        public Version WithHeight(int height, IReadOnlyCollection<string> defaultPreReleaseIdentifiers)
        {
            var preReleaseIdentifiers = this.preReleaseIdentifiers.Count > 0 || height == 0
                ? this.preReleaseIdentifiers
                : defaultPreReleaseIdentifiers;

            return this.preReleaseIdentifiers.Count == 0 && height > 0
                ? new Version(this.Major, this.Minor, this.Patch + 1, preReleaseIdentifiers, height, default)
                : new Version(this.Major, this.Minor, this.Patch, preReleaseIdentifiers, height,
                    height == 0 ? this.buildMetadata : default);
        }

        public Version AddBuildMetadata(string buildMetadata)
        {
            var separator = !string.IsNullOrEmpty(this.buildMetadata) && !string.IsNullOrEmpty(buildMetadata) ? "." : "";
            return new Version(this.Major, this.Minor, this.Patch, this.preReleaseIdentifiers, this.height, $"{this.buildMetadata}{separator}{buildMetadata}");
        }

        public bool IsBefore(int major, int minor) => this.Major < major || (this.Major == major && this.Minor < minor);

        public static Version ParseOrDefault(string text, string prefix) =>
            text == default || !text.StartsWith(prefix ?? "") ? default : ParseOrDefault(text.Substring(prefix?.Length ?? 0));

        private static Version ParseOrDefault(string text)
        {
            var dash = text.IndexOf('-');
            var plus = text.IndexOf('+');

            var meta = plus >= 0 ? plus : default(int?);
            var pre = dash >= 0 && (!meta.HasValue || dash < meta.Value) ? dash : default(int?);

            return ParseOrDefault(text.Before(meta).Before(pre).Split('.'), text.Before(meta).After(pre)?.Split('.'), text.After(meta));
        }

        private static Version ParseOrDefault(string[] numbers, IReadOnlyCollection<string> pre, string meta) =>
            numbers?.Length == 3 &&
                    int.TryParse(numbers[0], out var major) &&
                    int.TryParse(numbers[1], out var minor) &&
                    int.TryParse(numbers[2], out var patch)
                ? new Version(major, minor, patch, pre, default, meta)
                : default;
    }
}
