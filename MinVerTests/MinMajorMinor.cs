namespace MinVerTests
{
    using LibGit2Sharp;
    using MinVer.Lib;
    using MinVerTests.Infra;
    using Xbehave;
    using Xunit;

    using static MinVerTests.Infra.FileSystem;
    using static MinVerTests.Infra.Git;

    using Version = MinVer.Lib.Version;

    public static class MinMajorMinor
    {
        [Scenario]
        public static void NoCommits(string path, Repository repo, Version actualVersion)
        {
            $"Given an empty git repository in '{path = GetScenarioDirectory($"minimum-major-minor-not-tagged")}'"
                .x(c => repo = EnsureEmptyRepository(path).Using(c));

            "When the version is determined using minimum major minor '1.2'"
                .x(() => actualVersion = Versioner.GetVersion(new Repository(path), default, new MinVer.Lib.MajorMinor(1, 2), default, new TestLogger()));

            $"Then the version is '1.2.0-alpha.0'"
                .x(() => Assert.Equal("1.2.0-alpha.0", actualVersion.ToString()));
        }

        [Scenario]
        [Example("2.0.0", 1, 0, "2.0.0")]
        [Example("2.0.0", 2, 0, "2.0.0")]
        [Example("2.0.0", 3, 0, "3.0.0-alpha.0")]
        public static void Tagged(string tag, int major, int minor, string expectedVersion, string path, Repository repo, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"minimum-major-minor-tagged-{tag}-{major}-{minor}")}'"
                .x(c => repo = EnsureEmptyRepositoryAndCommit(path).Using(c));

            $"And the commit is tagged '{tag}'"
                .x(() => repo.ApplyTag(tag));

            $"When the version is determined using minimum major minor '{major}.{minor}'"
                .x(() => actualVersion = Versioner.GetVersion(new Repository(path), default, new MinVer.Lib.MajorMinor(major, minor), default, new TestLogger()));

            $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }

        [Scenario]
        public static void NotTagged(string path, Repository repo, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"minimum-major-minor-not-tagged")}'"
                .x(c => repo = EnsureEmptyRepositoryAndCommit(path).Using(c));

            "When the version is determined using minimum major minor '1.0'"
                .x(() => actualVersion = Versioner.GetVersion(new Repository(path), default, new MinVer.Lib.MajorMinor(1, 0), default, new TestLogger()));

            $"Then the version is '1.0.0-alpha.0'"
                .x(() => Assert.Equal("1.0.0-alpha.0", actualVersion.ToString()));
        }
    }
}
