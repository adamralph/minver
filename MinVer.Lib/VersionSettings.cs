namespace MinVer.Lib
{
    using System.Collections.Generic;

    public sealed class VersionSettings
    {
        public VersionSettings(IReadOnlyCollection<string> defaultPreReleaseIdentifiers = null)
        {
            this.DefaultPreReleaseIdentifiers = defaultPreReleaseIdentifiers ?? new[] {"alpha", "0"};
        }

        public IReadOnlyCollection<string> DefaultPreReleaseIdentifiers { get; }
    }
}
