namespace MinVer.Lib
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal class Repository
    {
        private readonly Commit head;
        private readonly IEnumerable<Tag> tags;

        private Repository(Commit head, IEnumerable<Tag> tags)
        {
            this.head = head;
            this.tags = tags;
        }

        public static bool TryCreateRepo(string workDir, out Repository repository, ILogger log)
        {
            repository = null;

            if (!Git.IsWorkingDirectory(workDir, log))
            {
                return false;
            }

            var head = Git.GetHeadOrDefault(workDir, log);
            var tags = head != null ? Git.GetTagsOrEmpty(workDir, log) : Enumerable.Empty<Tag>();

            repository = new Repository(head, tags);

            return true;
        }

        public Version GetVersion(string tagPrefix, VersionPart autoIncrement, string defaultPreReleasePhase, ILogger log)
        {
            var commit = this.head;

            if (commit == null)
            {
                var version = new Version(defaultPreReleasePhase);

                log.Info($"No commits found. Using default version {version}.");

                return version;
            }

            var tagsAndVersions = this.tags
                .Select(tag => (tag, Version.ParseOrDefault(tag.Name, tagPrefix)))
                .OrderBy(tagAndVersion => tagAndVersion.Item2)
                .ThenBy(tagsAndVersion => tagsAndVersion.tag.Name)
                .ToList();

            var commitsChecked = new HashSet<string>();
            var count = 0;
            var height = 0;
            var candidates = new List<Candidate>();
            var commitsToCheck = new Stack<(Commit, int, Commit)>();
            Commit previousCommit = null;

            if (log.IsTraceEnabled)
            {
                log.Trace($"Starting at commit {commit.ShortSha} (height {height})...");
            }

            while (true)
            {
                var parentCount = 0;

                if (commitsChecked.Add(commit.Sha))
                {
                    ++count;

                    var commitTagsAndVersions = tagsAndVersions.Where(tagAndVersion => tagAndVersion.tag.Sha == commit.Sha).ToList();
                    var foundVersion = false;

                    foreach (var (tag, commitVersion) in commitTagsAndVersions)
                    {
                        var candidate = new Candidate { Commit = commit, Height = height, Tag = tag.Name, Version = commitVersion, };

                        foundVersion = foundVersion || candidate.Version != null;

                        if (log.IsTraceEnabled)
                        {
                            log.Trace($"Found {(candidate.Version == null ? "non-" : null)}version tag {candidate}.");
                        }

                        candidates.Add(candidate);
                    }

                    if (!foundVersion)
                    {
                        if (log.IsTraceEnabled)
                        {
                            var parentIndex = 0;
                            Commit firstParent = null;

                            foreach (var parent in commit.Parents)
                            {
                                switch (parentIndex)
                                {
                                    case 0:
                                        firstParent = parent;
                                        break;
                                    case 1:
                                        log.Trace($"History diverges from {commit.ShortSha} (height {height}) to:");
                                        log.Trace($"- {firstParent.ShortSha} (height {height + 1})");
                                        goto default;
                                    default:
                                        log.Trace($"- {parent.ShortSha} (height {height + 1})");
                                        break;
                                }

                                ++parentIndex;
                                parentCount = parentIndex;
                            }
                        }

                        foreach (var parent in ((IEnumerable<Commit>)commit.Parents).Reverse())
                        {
                            commitsToCheck.Push((parent, height + 1, commit));
                        }

                        if (commitsToCheck.Count == 0 || commitsToCheck.Peek().Item2 <= height)
                        {
                            var candidate = new Candidate { Commit = commit, Height = height, Tag = null, Version = new Version(defaultPreReleasePhase), };

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
                        log.Trace($"History converges from {previousCommit.ShortSha} (height {height - 1}) back to previously seen commit {commit.ShortSha} (height {height}). Abandoning path.");
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
                        log.Trace($"Following path from {child.ShortSha} (height {height - 1}) through first parent {commit.ShortSha} (height {height})...");
                    }
                    else if (height <= oldHeight)
                    {
                        if (commitsToCheck.Any() && commitsToCheck.Peek().Item2 == height)
                        {
                            log.Trace($"Backtracking to {child.ShortSha} (height {height - 1}) and following path through next parent {commit.ShortSha} (height {height})...");
                        }
                        else
                        {
                            log.Trace($"Backtracking to {child.ShortSha} (height {height - 1}) and following path through last parent {commit.ShortSha} (height {height})...");
                        }
                    }
                }
            }

            log.Debug($"{count:N0} commits checked.");

            var orderedCandidates = candidates.OrderBy(candidate => candidate.Version).ToList();

            var tagWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Tag?.Length ?? 2) : 0;
            var versionWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Version?.ToString().Length ?? 4) : 0;
            var heightWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Height).ToString(CultureInfo.CurrentCulture).Length : 0;

            if (log.IsDebugEnabled)
            {
                foreach (var candidate in orderedCandidates.Take(orderedCandidates.Count - 1))
                {
                    log.Debug($"Ignoring {candidate.ToString(tagWidth, versionWidth, heightWidth)}.");
                }
            }

            var selectedCandidate = orderedCandidates.Last();

            if (selectedCandidate.Tag == null)
            {
                log.Info($"No commit found with a valid SemVer 2.0 version{(tagPrefix == null ? null : $" prefixed with '{tagPrefix}'")}. Using default version {selectedCandidate.Version}.");
            }

            log.Info($"Using{(log.IsDebugEnabled && orderedCandidates.Count > 1 ? "    " : " ")}{selectedCandidate.ToString(tagWidth, versionWidth, heightWidth)}.");

            return selectedCandidate.Version.WithHeight(selectedCandidate.Height, autoIncrement, defaultPreReleasePhase);
        }

        private class Candidate
        {
            public Commit Commit { get; set; }

            public int Height { get; set; }

            public string Tag { get; set; }

            public Version Version { get; set; }

            public override string ToString() => this.ToString(0, 0, 0);

            public string ToString(int tagWidth, int versionWidth, int heightWidth) =>
                $"{{ {nameof(this.Commit)}: {this.Commit.ShortSha}, {nameof(this.Tag)}: {$"{(this.Tag == null ? "null" : $"'{this.Tag}'")},".PadRight(tagWidth + 3)} {nameof(this.Version)}: {$"{this.Version?.ToString() ?? "null"},".PadRight(versionWidth + 1)} {nameof(this.Height)}: {this.Height.ToString(CultureInfo.CurrentCulture).PadLeft(heightWidth)} }}";
        }
    }
}
