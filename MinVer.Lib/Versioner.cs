namespace MinVer.Lib
{
    public static class Versioner
    {
        public static Version GetVersion(string repoOrWorkDir, string tagPrefix, MajorMinor minMajorMinor, string buildMeta, VersionPart autoIncrement, string defaultPreReleasePhase, ILogger log)
        {
            if (log == null)
            {
                throw new System.ArgumentNullException(nameof(log));
            }

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
                if (minMajorMinor != default)
                {
                    log.Debug($"The calculated version {calculatedVersion} satisfies the minimum major minor {minMajorMinor}.");
                }
            }

            log.Info($"Calculated version {calculatedVersion}.");

            return calculatedVersion;
        }

        private static Version GetVersion(string repoOrWorkDir, string tagPrefix, VersionPart autoIncrement, string defaultPreReleasePhase, ILogger log)
        {
            if (!RepositoryEx.TryCreateRepo(repoOrWorkDir, out var repo))
            {
                var version = new Version(defaultPreReleasePhase);

                log.Warn(1001, $"'{repoOrWorkDir}' is not a valid repository or working directory. Using default version {version}.");

                return version;
            }

            try
            {
                return repo.GetVersion(tagPrefix, autoIncrement, defaultPreReleasePhase, log);
            }
            finally
            {
                repo.Dispose();
            }
        }
    }
}
