namespace MinVer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LibGit2Sharp;

    public static class Versioner
    {
        public static Version GetVersion(string path)
        {
            using (var repo = new Repository(path))
            {
                return GetVersion(repo.Commits.FirstOrDefault(), repo.Tags.ToList());
            }
        }

        private static Version GetVersion(Commit commit, List<Tag> tags)
        {
            if (commit == default)
            {
                return new Version();
            }

            var commitsChecked = new HashSet<string>();
            var count = 0;
            var height = 0;
            var candidates = new List<Candidate>();
            var commitsToCheck = new Stack<Tuple<Commit, int>>();

            while (true)
            {
                if (commitsChecked.Add(commit.Sha))
                {
                    ++count;

                    var commitVersion = GetVersionOrDefault(tags, commit);

                    if (commitVersion != default)
                    {
                        var candidate = new Candidate { Version = commitVersion, Commit = commit, Height = height };
                        Log($"Detected {candidate}.");
                        candidates.Add(candidate);
                    }
                    else
                    {
                        foreach (var parent in commit.Parents.Reverse())
                        {
                            commitsToCheck.Push(Tuple.Create(parent, height + 1));
                        }

                        if (commitsToCheck.Count == 0 || commitsToCheck.Peek().Item2 <= height)
                        {
                            var candidate = new Candidate { Version = new Version(), Commit = commit, Height = height };
                            Log($"Inferred {candidate}.");
                            candidates.Add(candidate);
                        }
                    }
                }

                if (commitsToCheck.Count == 0)
                {
                    break;
                }

                (commit, height) = commitsToCheck.Pop();
            }

            Log($"{count:N0} commits checked.");

            var orderedCandidates = candidates.OrderBy(candidate => candidate.Version).ToList();

            foreach (var candidate in orderedCandidates.Take(orderedCandidates.Count - 1))
            {
                Log($"Ignoring {candidate}...");
            }

            var selectedCandidate = orderedCandidates.Last();
            Log($"Using {selectedCandidate}.");

            var calculatedVersion = selectedCandidate.Version.AddHeight(selectedCandidate.Height);
            Log($"Calculated {calculatedVersion}.");

            return calculatedVersion;
        }

        private class Candidate
        {
            public Version Version { get; set; }

            public Commit Commit { get; set; }

            public int Height { get; set; }

            public override string ToString() => $"{{ {nameof(this.Version)}: {this.Version}, {nameof(this.Commit)}: {this.Commit}, {nameof(this.Height)}: {this.Height} }}";
        }

        private static void Log(string message) => Console.Error.WriteLine($"MinVer: {message}");

        private static Version GetVersionOrDefault(List<Tag> tags, Commit commit) => tags
            .Where(tag => tag.Target.Sha == commit.Sha)
            .Select(tag => Version.ParseOrDefault(tag.FriendlyName))
            .Where(_ => _ != default)
            .OrderByDescending(_ => _)
            .FirstOrDefault();
    }
}
