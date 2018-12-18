namespace MinVer.Lib
{
    public static class Versioner
    {
        public static Version GetVersion(string repoOrWorkDir, string tagPrefix, MajorMinor minMajorMinor, string buildMeta, ILogger log)
        {
            var version = GetVersion(repoOrWorkDir, tagPrefix, log).AddBuildMetadata(buildMeta);

            var calculatedVersion = version.Satisfying(minMajorMinor);

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

            log.Debug($"Calculated version {calculatedVersion}.");

            return calculatedVersion;
        }

        private static Version GetVersion(string repoOrWorkDir, string tagPrefix, ILogger log)
        {
            if (!RepositoryEx.TryCreateRepo(repoOrWorkDir, out var repo))
            {
                var version = new Version();

                log.Warn(1001, $"'{repoOrWorkDir}' is not a valid repository or working directory. Using default version {version}.");

                return version;
            }

            try
            {
                return repo.GetVersion(tagPrefix, log);
            }
            finally
            {
                repo.Dispose();
            }
        }
    }
}
