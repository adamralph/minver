using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public sealed class RestrictedTheory : TheoryAttribute
    {
        private readonly List<OSPlatform> typedExcludedOsPlatforms;

        public RestrictedTheory(string[] excludedSdkVersionPatterns, string[] excludedOsPlatforms, string reason) =>
            (
                this.ExcludedSdkVersionPatterns,
                this.ExcludedOsPlatforms,
                this.typedExcludedOsPlatforms,
                this.Reason) =
            (
                excludedSdkVersionPatterns,
                excludedOsPlatforms,
                excludedOsPlatforms.Select(OSPlatform.Create).ToList(),
                reason);

        public string[] ExcludedSdkVersionPatterns { get; }

        public string[] ExcludedOsPlatforms { get; }

        public string Reason { get; }

        public override string Skip
        {
            get =>
                this.ExcludedSdkVersionPatterns.Any(pattern => Regex.IsMatch(Sdk.Version, pattern)) ||
                this.typedExcludedOsPlatforms.Any(RuntimeInformation.IsOSPlatform)
                    ? this.Reason
                    : base.Skip;

            set => base.Skip = value;
        }
    }
}
