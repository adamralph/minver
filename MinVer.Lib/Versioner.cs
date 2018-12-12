namespace MinVer.Lib
{
    using System.Collections.Generic;
    using System.Linq;
    using LibGit2Sharp;

    public static class Versioner
    {
        public static Version GetVersion(Repository repo, string tagPrefix, MajorMinor minMajorMinor, IReadOnlyCollection<string> defaultPrereleaseIdentifiers, string buildMetadata, ILogger log)
        {
            defaultPrereleaseIdentifiers = defaultPrereleaseIdentifiers ?? new[] {"alpha", "0"};
            var commit = repo.Commits.FirstOrDefault();

            if (commit == default)
            {
                var version = new Version(minMajorMinor?.Major ?? 0, minMajorMinor?.Minor ?? 0, defaultPrereleaseIdentifiers, buildMetadata);

                log.Info($"No commits found. Using default version {version}.");

                return version;
            }

            var tagsAndVersions = repo.Tags
                .Select(tag => (tag, Version.ParseOrDefault(tag.FriendlyName, tagPrefix)))
                .OrderByDescending(tagAndVersion => tagAndVersion.Item2)
                .ToList();

            var commitsChecked = new HashSet<string>();
            var count = 0;
            var height = 0;
            var candidates = new List<Candidate>();
            var commitsToCheck = new Stack<(Commit, int, Commit)>();
            Commit previousCommit = default;

            if (log.IsTraceEnabled)
            {
                log.Trace($"Starting at commit {commit.ShortSha()} (height {height})...");
            }

            while (true)
            {
                var parentCount = 0;

                if (commitsChecked.Add(commit.Sha))
                {
                    ++count;

                    var (tag, commitVersion) = tagsAndVersions.FirstOrDefault(tagAndVersion => tagAndVersion.tag.Target.Sha == commit.Sha);

                    if (commitVersion != default)
                    {
                        var candidate = new Candidate { Commit = commit, Height = height, Tag = tag.FriendlyName, Version = commitVersion, };

                        if (log.IsTraceEnabled)
                        {
                            log.Trace($"Found version tag {candidate}.");
                        }

                        candidates.Add(candidate);
                    }
                    else
                    {
                        if (tag != default)
                        {
                            var candidate = new Candidate { Commit = commit, Height = height, Tag = tag.FriendlyName, Version = default, };

                            if (log.IsTraceEnabled)
                            {
                                log.Trace($"Found non-version tag {candidate}.");
                            }

                            candidates.Add(candidate);
                        }

                        if (log.IsTraceEnabled)
                        {
                            var parentIndex = 0;
                            Commit firstParent = default;

                            foreach (var parent in commit.Parents)
                            {
                                switch (parentIndex)
                                {
                                    case 0:
                                        firstParent = parent;
                                        break;
                                    case 1:
                                        log.Trace($"History diverges from {commit.ShortSha()} (height {height}) to:");
                                        log.Trace($"- {firstParent.ShortSha()} (height {height + 1})");
                                        goto default;
                                    default:
                                        log.Trace($"- {parent.ShortSha()} (height {height + 1})");
                                        break;
                                }

                                ++parentIndex;
                                parentCount = parentIndex;
                            }
                        }

                        foreach (var parent in commit.Parents.Reverse())
                        {
                            commitsToCheck.Push((parent, height + 1, commit));
                        }

                        if (commitsToCheck.Count == 0 || commitsToCheck.Peek().Item2 <= height)
                        {
                            var candidate = new Candidate { Commit = commit, Height = height, Tag = default, Version = new Version(defaultPrereleaseIdentifiers), };

                            if (log.IsTraceEnabled)
                            {
                                log.Trace($"Found root commit {candidate}.");
                            }

                            candidates.Add(candidate);
                        }
                    }
                }
                else
                {
                    if (log.IsTraceEnabled)
                    {
                        log.Trace($"History converges from {previousCommit.ShortSha()} (height {height - 1}) back to previously seen commit {commit.ShortSha()} (height {height}). Abandoning path.");
                    }
                }

                if (commitsToCheck.Count == 0)
                {
                    break;
                }

                if (log.IsTraceEnabled)
                {
                    previousCommit = commit;
                }

                var oldHeight = height;
                Commit child;
                (commit, height, child) = commitsToCheck.Pop();

                if (log.IsTraceEnabled)
                {
                    if (parentCount > 1)
                    {
                        log.Trace($"Following path from {child.ShortSha()} (height {height - 1}) through first parent {commit.ShortSha()} (height {height})...");
                    }
                    else if (height <= oldHeight)
                    {
                        if (commitsToCheck.Any() && commitsToCheck.Peek().Item2 == height)
                        {
                            log.Trace($"Backtracking to {child.ShortSha()} (height {height - 1}) and following path through next parent {commit.ShortSha()} (height {height})...");
                        }
                        else
                        {
                            log.Trace($"Backtracking to {child.ShortSha()} (height {height - 1}) and following path through last parent {commit.ShortSha()} (height {height})...");
                        }
                    }
                }
            }

            log.Debug($"{count:N0} commits checked.");

            var orderedCandidates = candidates.OrderBy(candidate => candidate.Version).ToList();

            var tagWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Tag?.Length ?? 2) : 0;
            var versionWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Version?.ToString().Length ?? 4) : 0;
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

            var baseVersion = minMajorMinor != default && selectedCandidate.Version.IsBefore(minMajorMinor.Major, minMajorMinor.Minor)
                ? new Version(minMajorMinor.Major, minMajorMinor.Minor, defaultPrereleaseIdentifiers)
                : selectedCandidate.Version;

            if (baseVersion != selectedCandidate.Version)
            {
                log.Info($"Bumping version to {baseVersion} to satisfy minimum major minor {minMajorMinor}.");
            }

            var calculatedVersion = baseVersion.WithHeight(selectedCandidate.Height, defaultPrereleaseIdentifiers).AddBuildMetadata(buildMetadata);
            log.Debug($"Calculated version {calculatedVersion}.");

            return calculatedVersion;
        }

        public static string ShortSha(this Commit commit) => commit.Sha.Substring(0, 7);

        private class Candidate
        {
            public Commit Commit { get; set; }

            public int Height { get; set; }

            public string Tag { get; set; }

            public Version Version { get; set; }

            public override string ToString() => this.ToString(0, 0, 0);

            public string ToString(int tagWidth, int versionWidth, int heightWidth) =>
                $"{{ {nameof(this.Commit)}: {this.Commit.ShortSha()}, {nameof(this.Tag)}: {$"{(this.Tag == default ? "null" : $"'{this.Tag}'")},".PadRight(tagWidth + 3)} {nameof(this.Version)}: {$"{this.Version?.ToString() ?? "null"},".PadRight(versionWidth + 1)} {nameof(this.Height)}: {this.Height.ToString().PadLeft(heightWidth)} }}";
        }
    }
}
