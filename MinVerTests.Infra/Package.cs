#if NET
using System.Collections.Generic;
using System.Linq;

namespace MinVerTests.Infra
{
    public record Package(string Version, AssemblyVersion AssemblyVersion, FileVersion FileVersion)
    {
        public static Package WithVersion(int Major, int Minor, int Patch, IEnumerable<string>? PreReleaseIdentifiers = null, int Height = 0, string BuildMetadata = "")
        {
            var version = $"{Major}.{Minor}.{Patch}{(!(PreReleaseIdentifiers?.Any() ?? false) ? "" : $"-{string.Join(".", PreReleaseIdentifiers)}")}{(Height == 0 ? "" : $".{Height}")}{(string.IsNullOrEmpty(BuildMetadata) ? "" : $"+{BuildMetadata}")}";

            return new Package(version, new AssemblyVersion(Major, 0, 0, 0), new FileVersion(Major, Minor, Patch, 0, version));
        }
    }
}
#endif
