namespace MinVer
{
    using System.Linq;
    using LibGit2Sharp;

    public static class Versioner
    {
        public static Version GetVersion(string path)
        {
            using (var repo = new Repository(path))
            {
                return GetVersion(repo.Commits.FirstOrDefault(), 0, repo.Tags);
            }
        }

        private static Version GetVersion(Commit commit, int height, TagCollection tags)
        {
            if (commit == default)
            {
                return new Version();
            }

            var version = GetVersionOrDefault(tags, commit);

            if (version != default)
            {
                return version.AddHeight(height);
            }

            return commit.Parents
                    .Select(parent => GetVersion(parent, height + 1, tags))
                    .Where(_ => _ != default)
                    .OrderByDescending(_ => _)
                    .FirstOrDefault() ??
                new Version().AddHeight(height);
        }

        private static Version GetVersionOrDefault(TagCollection tags, Commit commit) => tags
            .Where(tag => tag.Target.Sha == commit.Sha)
            .Select(tag => Version.ParseOrDefault(tag.FriendlyName))
            .Where(_ => _ != default)
            .OrderByDescending(_ => _)
            .FirstOrDefault();
    }
}
