namespace MinVer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LibGit2Sharp;

    public static class Versioner
    {
        public static Version GetVersion(string path, string tagPrefix)
        {
            // Repository.ctor(string) throws RepositoryNotFoundException in this case
            if (!Directory.Exists(path))
            {
                // Substring of RepositoryNotFoundException.Message $"Path '{path}' doesn't point at a valid Git repository or workdir."
                throw new Exception($"Path '{path}' doesn't point at a valid workdir.");
            }

            var testPath = path;
            while (testPath != null)
            {
                try
                {
                    using (var repo = new Repository(testPath))
                    {
                        return GetVersion(repo.Commits.FirstOrDefault(), repo.Tags.ToList(), tagPrefix);
                    }
                }
                catch (RepositoryNotFoundException)
                {
                    testPath = Directory.GetParent(testPath)?.FullName;
                }
            }

            // Includes substring of RepositoryNotFoundException.Message $"Path '{path}' doesn't point at a valid Git repository or workdir."
            Log($"WARNING: Using default version. Path '{path}' doesn't point at a valid Git repository.");
            return new Version();
        }

        private static Version GetVersion(Commit commit, List<Tag> tags, string tagPrefix)
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

                    var (tag, commitVersion) = GetVersionOrDefault(tags, commit, tagPrefix);

                    if (commitVersion != default)
                    {
                        candidates.Add(new Candidate { Sha = commit.Sha, Height = height, Tag = tag, Version = commitVersion, });
                    }
                    else
                    {
                        foreach (var parent in commit.Parents.Reverse())
                        {
                            commitsToCheck.Push(Tuple.Create(parent, height + 1));
                        }

                        if (commitsToCheck.Count == 0 || commitsToCheck.Peek().Item2 <= height)
                        {
                            candidates.Add(new Candidate { Sha = commit.Sha, Height = height, Tag = "(none)", Version = new Version(), });
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

            var heightWidth = orderedCandidates.Max(candidate => candidate.Height).ToString().Length;
            var tagWidth = orderedCandidates.Max(candidate => candidate.Tag.Length);

            foreach (var candidate in orderedCandidates.Take(orderedCandidates.Count - 1))
            {
                Log($"Ignoring commit {candidate.ToString(heightWidth, tagWidth)}");
            }

            var selectedCandidate = orderedCandidates.Last();
            Log($"Using commit    {selectedCandidate.ToString(heightWidth, tagWidth)}");

            var calculatedVersion = selectedCandidate.Version.AddHeight(selectedCandidate.Height);
            Log($"Calculated version {calculatedVersion}");

            return calculatedVersion;
        }

        private class Candidate
        {
            public string Sha { get; set; }

            public int Height { get; set; }

            public string Tag { get; set; }

            public Version Version { get; set; }

            public string ToString(int heightWidth, int tagWidth) =>
                $"{{ {nameof(this.Sha)}: {this.Sha}, {nameof(this.Height)}: {this.Height.ToString().PadLeft(heightWidth)}, {nameof(this.Tag)}: {$"'{this.Tag}'".PadLeft(tagWidth + 2)}, {nameof(this.Version)}: {this.Version.ToString()} }}";
        }

        private static void Log(string message) => Console.Error.WriteLine($"MinVer: {message}");

        private static (string, Version) GetVersionOrDefault(List<Tag> tags, Commit commit, string tagPrefix) => tags
            .Where(tag => tag.Target.Sha == commit.Sha)
            .Select(tag => (tag.FriendlyName, Version.ParseOrDefault(tag.FriendlyName, tagPrefix)))
            .Where(tuple => tuple.Item2 != default)
            .OrderByDescending(tuple => tuple.Item2)
            .FirstOrDefault();
    }
}
