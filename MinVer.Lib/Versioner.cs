namespace MinVer.Lib
{
    public static class Versioner
    {
        public static Version GetVersion(string workDir, string tagPrefix, MajorMinor minMajorMinor, string buildMeta, VersionPart autoIncrement, string defaultPreReleasePhase, ILogger log)
        {
            log = log ?? new NullLogger();

            defaultPreReleasePhase = string.IsNullOrEmpty(defaultPreReleasePhase)
                ? "alpha"
                : defaultPreReleasePhase;

            var version = GetVersion(workDir, tagPrefix, autoIncrement, defaultPreReleasePhase, log).AddBuildMetadata(buildMeta);

            var calculatedVersion = version.Satisfying(minMajorMinor, defaultPreReleasePhase);

            if (calculatedVersion != version)
            {
                log.Info($"Bumping version to {calculatedVersion} to satisfy minimum major minor {minMajorMinor}.");
            }
            else
            {
                if (minMajorMinor != null)
                {
                    log.Debug($"The calculated version {calculatedVersion} satisfies the minimum major minor {minMajorMinor}.");
                }
            }

            log.Info($"Calculated version {calculatedVersion}.");

            return calculatedVersion;
        }

        private static Version GetVersion(string workDir, string tagPrefix, VersionPart autoIncrement, string defaultPreReleasePhase, ILogger log)
        {
            if (!Repository.TryCreateRepo(workDir, out var repo, log))
            {
                var version = new Version(defaultPreReleasePhase);

                log.Warn(1001, $"'{workDir}' is not a valid Git working directory. Using default version {version}.");

                return version;
            }

            return repo.GetVersion(tagPrefix, autoIncrement, defaultPreReleasePhase, log);
        }
    }
}
