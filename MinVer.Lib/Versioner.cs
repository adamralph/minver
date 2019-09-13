namespace MinVer.Lib
{
    public static class Versioner
    {
        public static Version GetVersion(string repoOrWorkDir, string tagPrefix, MajorMinor minMajorMinor, string buildMeta, VersionPart autoIncrement, string defaultPreReleasePhase, ILogger log)
        {
            log = log ?? new NullLogger();

            defaultPreReleasePhase = string.IsNullOrEmpty(defaultPreReleasePhase)
                ? "alpha"
                : defaultPreReleasePhase;

            var version = GetVersion(repoOrWorkDir, tagPrefix, autoIncrement, defaultPreReleasePhase, log).AddBuildMetadata(buildMeta);

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

        private static Version GetVersion(string repoOrWorkDir, string tagPrefix, VersionPart autoIncrement, string defaultPreReleasePhase, ILogger log)
        {
#pragma warning disable IDE0068 // Use recommended dispose pattern
            if (!RepositoryEx.TryCreateRepo(repoOrWorkDir, out var repo))
#pragma warning restore IDE0068 // Use recommended dispose pattern
            {
                var version = new Version(defaultPreReleasePhase);

                log.Warn(1001, $"'{repoOrWorkDir}' is not a valid repository or working directory. Using default version {version}.");

                return version;
            }

            using (repo)
            {
                return repo.GetVersion(tagPrefix, autoIncrement, defaultPreReleasePhase, log);
            }
        }
    }
}
