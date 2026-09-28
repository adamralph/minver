using System.Globalization;

namespace MinVer.Lib;

public static class Versioner
{
    public static async Task<Version> GetVersion(string workDir, string tagPrefix, MajorMinor minMajorMinor, string buildMeta, VersionPart autoIncrement, IReadOnlyCollection<string> defaultPreReleaseIdentifiers, bool ignoreHeight, ILogger log)
    {
        log = log ?? throw new ArgumentNullException(nameof(log));

        var (version, height, isFromTag) = await GetVersion(workDir, tagPrefix, defaultPreReleaseIdentifiers, log);

        _ = height.HasValue && ignoreHeight && log.IsDebugEnabled && log.Debug("Ignoring height.");
        version = !height.HasValue || ignoreHeight ? version : version.WithHeight(height.Value, autoIncrement, defaultPreReleaseIdentifiers);

        version = version.AddBuildMetadata(buildMeta);

        var ignoreMinMajorMinor = isFromTag && height is 0;

        var calculatedVersion =
            ignoreMinMajorMinor
            ? version.Satisfying(MajorMinor.Default, defaultPreReleaseIdentifiers)
            : version.Satisfying(minMajorMinor, defaultPreReleaseIdentifiers);

        _ = ignoreMinMajorMinor
            ? minMajorMinor != MajorMinor.Default && log.IsDebugEnabled && log.Debug($"Ignoring minimum major minor {minMajorMinor} because the commit is tagged.")
            : calculatedVersion != version
                ? log.IsInfoEnabled && log.Info($"Bumping version to {calculatedVersion} to satisfy minimum major minor {minMajorMinor}.")
                : log.IsDebugEnabled && log.Debug($"The calculated version {calculatedVersion} satisfies the minimum major minor {minMajorMinor}.");

        _ = log.IsInfoEnabled && log.Info($"Calculated version {calculatedVersion}.");

        return calculatedVersion;
    }

    private static async Task<(Version Version, int? Height, bool IsFromTag)> GetVersion(string workDir, string tagPrefix, IReadOnlyCollection<string> defaultPreReleaseIdentifiers, ILogger log)
    {
        if (!await Git.IsWorkingDirectory(workDir, log))
        {
            var version = new Version(defaultPreReleaseIdentifiers);

            _ = log.IsWarnEnabled && log.Warn(1001, $"'{workDir}' is not a valid Git working directory. Using default version {version}.");

            return (version, null, false);
        }

        if (await Git.TryGetHead(workDir, log) is not { } head)
        {
            var version = new Version(defaultPreReleaseIdentifiers);

            _ = log.IsInfoEnabled && log.Info($"No commits found. Using default version {version}.");

            return (version, null, false);
        }

        var tags = await Git.GetTags(workDir, log);

        var orderedCandidates = GetCandidates(head, tags, tagPrefix, defaultPreReleaseIdentifiers, log)
            .OrderBy(candidate => candidate.Version)
            .ThenByDescending(candidate => candidate.Index).ToList();

        var tagWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Tag.Length) : 0;
        var versionWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Version.ToString().Length) : 0;
        var heightWidth = log.IsDebugEnabled ? orderedCandidates.Max(candidate => candidate.Height).ToString(CultureInfo.CurrentCulture).Length : 0;

        if (log.IsDebugEnabled)
        {
            foreach (var candidate in orderedCandidates.Take(orderedCandidates.Count - 1))
            {
                _ = log.Debug($"Ignoring {candidate.ToString(tagWidth, versionWidth, heightWidth)}.");
            }
        }

        var selectedCandidate = orderedCandidates.Last();

        _ = string.IsNullOrEmpty(selectedCandidate.Tag) && log.IsInfoEnabled && log.Info($"No commit found with a valid SemVer 2.0 version{(string.IsNullOrEmpty(tagPrefix) ? "" : $" prefixed with '{tagPrefix}'")}. Using default version {selectedCandidate.Version}.");
        _ = log.IsInfoEnabled && log.Info($"Using{(log.IsDebugEnabled && orderedCandidates.Count > 1 ? "    " : " ")}{selectedCandidate.ToString(tagWidth, versionWidth, heightWidth)}.");

        return (selectedCandidate.Version, selectedCandidate.Height, !string.IsNullOrEmpty(selectedCandidate.Tag));
    }

    private static List<Candidate> GetCandidates(Commit head, IEnumerable<(string Name, string Sha)> tags, string tagPrefix, IReadOnlyCollection<string> defaultPreReleaseIdentifiers, ILogger log)
    {
        var tagsAndVersions = new List<(string Name, string Sha, Version Version)>();

        foreach (var (name, sha) in tags)
        {
            if (Version.TryParse(name, out var version, tagPrefix))
            {
                tagsAndVersions.Add((name, sha, version));
            }
            else
            {
                _ = log.IsDebugEnabled && log.Debug($"Ignoring non-version tag {{ Name: {name}, Sha: {sha} }}.");
            }
        }

        tagsAndVersions =
        [
            .. tagsAndVersions
                .OrderBy(tagAndVersion => tagAndVersion.Version)
                .ThenBy(tagsAndVersion => tagsAndVersion.Name),
        ];

        var itemsToCheck = new Stack<(Commit Commit, int Height, Commit? Child)>();
        itemsToCheck.Push((head, 0, null));

        var checkedShas = new HashSet<string>();
        var candidates = new List<Candidate>();

        while (itemsToCheck.TryPop(out var item))
        {
            _ = item.Child != null && log.IsTraceEnabled && log.Trace($"Checking parents of commit {item.Child}...");
            _ = log.IsTraceEnabled && log.Trace($"Checking commit {item.Commit} (height {item.Height})...");

            if (!checkedShas.Add(item.Commit.Sha))
            {
                _ = log.IsTraceEnabled && log.Trace($"Commit {item.Commit} already checked. Abandoning path.");
                continue;
            }

            var commitTagsAndVersions = tagsAndVersions.Where(tagAndVersion => tagAndVersion.Sha == item.Commit.Sha).ToList();

            if (commitTagsAndVersions.Count != 0)
            {
                foreach (var (name, _, version) in commitTagsAndVersions)
                {
                    var candidate = new Candidate(item.Commit, item.Height, name, version, candidates.Count);
                    _ = log.IsTraceEnabled && log.Trace($"Found version tag {candidate}.");
                    candidates.Add(candidate);
                }

                continue;
            }

            _ = log.IsTraceEnabled && log.Trace($"Found no version tags on commit {item.Commit}.");

            if (item.Commit.Parents.Count == 0)
            {
                candidates.Add(new Candidate(item.Commit, item.Height, "", new Version(defaultPreReleaseIdentifiers), candidates.Count));
                _ = log.IsTraceEnabled && log.Trace($"Found root commit {candidates.Last()}.");
                continue;
            }

            if (log.IsTraceEnabled)
            {
                _ = log.Trace($"Commit {item.Commit} has {item.Commit.Parents.Count} parent(s):");
                foreach (var parent in item.Commit.Parents)
                {
                    _ = log.Trace($"- {parent}");
                }
            }

            foreach (var parent in ((IEnumerable<Commit>)item.Commit.Parents).Reverse())
            {
                itemsToCheck.Push((parent, item.Height + 1, item.Commit));
            }
        }

        _ = log.IsDebugEnabled && log.Debug($"{checkedShas.Count:N0} commits checked.");
        return candidates;
    }

    private sealed class Candidate(Commit commit, int height, string tag, Version version, int index)
    {
        public Commit Commit { get; } = commit;

        public int Height { get; } = height;

        public string Tag { get; } = tag;

        public Version Version { get; } = version;

        public int Index { get; } = index;

        public override string ToString() => ToString(0, 0, 0);

        public string ToString(int tagWidth, int versionWidth, int heightWidth) =>
            $"{{ {nameof(Commit)}: {Commit.ShortSha}, {nameof(Tag)}: {$"'{Tag}',".PadRight(tagWidth + 3)} {nameof(Version)}: {$"{Version},".PadRight(versionWidth + 1)} {nameof(Height)}: {Height.ToString(CultureInfo.CurrentCulture).PadLeft(heightWidth)} }}";
    }
}
