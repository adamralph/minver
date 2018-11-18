namespace MinVer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using LibGit2Sharp;

    public static class Versioner
    {
        public static Version GetVersion(Repository repo, string tagPrefix, MajorMinor range, string buildMetadata, IEnumerable<string> defaultPreReleaseIdentifiers, ILogger log)
        {
            log.Debug(() => $"MinVer {typeof(Versioner).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion}");

            var commit = repo.Commits.FirstOrDefault();

            if (commit == default)
            {
                return new Version(defaultPreReleaseIdentifiers);
            }

            var tagsAndVersions = repo.Tags
                .Select(tag => (tag, Version.ParseOrDefault(tag.FriendlyName, tagPrefix)))
                .Where(tagAndVersion => tagAndVersion.Item2 != default)
                .OrderByDescending(tagAndVersion => tagAndVersion.Item2)
                .ToList();

            var commitsChecked = new HashSet<string>();
            var count = 0;
            var height = 0;
            var candidates = new List<Candidate>();
            var commitsToCheck = new Stack<(Commit, int)>();

            while (true)
            {
                if (commitsChecked.Add(commit.Sha))
                {
                    ++count;

                    var (tag, commitVersion) = tagsAndVersions.FirstOrDefault(tagAndVersion => tagAndVersion.tag.Target.Sha == commit.Sha);

                    if (commitVersion != default)
                    {
                        candidates.Add(new Candidate { Commit = commit.Sha, Height = height, Tag = tag.FriendlyName, Version = commitVersion, });
                    }
                    else
                    {
                        foreach (var parent in commit.Parents.Reverse())
                        {
                            commitsToCheck.Push((parent, height + 1));
                        }

                        if (commitsToCheck.Count == 0 || commitsToCheck.Peek().Item2 <= height)
                        {
                            candidates.Add(new Candidate { Commit = commit.Sha, Height = height, Tag = "(none)", Version = new Version(defaultPreReleaseIdentifiers), });
                        }
                    }
                }

                if (commitsToCheck.Count == 0)
                {
                    break;
                }

                (commit, height) = commitsToCheck.Pop();
            }

            log.Debug($"{count:N0} commits checked.");

            var orderedCandidates = candidates.OrderBy(candidate => candidate.Version).ToList();

            var tagWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Tag.Length) : 0;
            var versionWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Version.ToString().Length) : 0;
            var heightWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Height).ToString().Length : 0;

            if (log.IsDebugEnabled)
            {
                foreach (var candidate in orderedCandidates.Take(orderedCandidates.Count - 1))
                {
                    log.Debug($"Ignoring {candidate.ToString(tagWidth, versionWidth, heightWidth)}.");
                }
            }

            var selectedCandidate = orderedCandidates.Last();
            log.Info($"Using{(log.IsDebugEnabled && orderedCandidates.Count > 1 ? "    " : " ")}{selectedCandidate.ToString(tagWidth, versionWidth, heightWidth)}.");

            var baseVersion = range != default && selectedCandidate.Version.IsBefore(range.Major, range.Minor)
                ? new Version(range.Major, range.Minor, defaultPreReleaseIdentifiers)
                : selectedCandidate.Version;

            if (baseVersion != selectedCandidate.Version)
            {
                log.Info($"Bumping version to {baseVersion} to satisfy {range} range.");
            }

            var calculatedVersion = baseVersion.WithHeight(selectedCandidate.Height, defaultPreReleaseIdentifiers).AddBuildMetadata(buildMetadata);
            log.Debug($"Calculated version {calculatedVersion}.");

            return calculatedVersion;
        }

        private class Candidate
        {
            public string Commit { get; set; }

            public int Height { get; set; }

            public string Tag { get; set; }

            public Version Version { get; set; }

            public string ToString(int tagWidth, int versionWidth, int heightWidth) =>
                $"{{ {nameof(this.Commit)}: {this.Commit.Substring(0, 7)}, {nameof(this.Tag)}: {$"'{this.Tag}',".PadRight(tagWidth + 3)} {nameof(this.Version)}: {$"{this.Version.ToString()},".PadRight(versionWidth + 1)} {nameof(this.Height)}: {this.Height.ToString().PadLeft(heightWidth)} }}";
        }
    }
}
