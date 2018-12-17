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
        public static void NoCommits(string path, Version actualVersion)
        {
            $"Given an empty git repository in '{path = GetScenarioDirectory($"minimum-major-minor-not-tagged")}'"
                .x(c => EnsureEmptyRepository(path).Using(c));

            "When the version is determined using minimum major minor '1.2'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, new MajorMinor(1, 2), default, new TestLogger()));

            $"Then the version is '1.2.0-alpha.0'"
                .x(() => Assert.Equal("1.2.0-alpha.0", actualVersion.ToString()));
        }

        [Scenario]
        [Example("2.0.0", 1, 0, "2.0.0", true)]
        [Example("2.0.0", 2, 0, "2.0.0", true)]
        [Example("2.0.0", 3, 0, "3.0.0-alpha.0", false)]
        public static void Tagged(string tag, int major, int minor, string expectedVersion, bool isRedundant, string path, Repository repo, TestLogger logger, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"minimum-major-minor-tagged-{tag}-{major}-{minor}")}'"
                .x(c => repo = EnsureEmptyRepositoryAndCommit(path).Using(c));

            $"And the commit is tagged '{tag}'"
                .x(() => repo.ApplyTag(tag));

            $"When the version is determined using minimum major minor '{major}.{minor}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, new MajorMinor(major, minor), default, logger = new TestLogger()));

            $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));

            if (isRedundant)
            {
                $"And a debug message is logged because the minimum major minor is redundant"
                    .x(() => Assert.Contains(logger.DebugMessages, message => message.Contains($"Minimum major minor {major}.{minor} is redundant. The calculated version is already equal or higher.")));
            }
        }

        [Scenario]
        public static void NotTagged(string path, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"minimum-major-minor-not-tagged")}'"
                .x(c => EnsureEmptyRepositoryAndCommit(path).Using(c));

            "When the version is determined using minimum major minor '1.0'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, new MajorMinor(1, 0), default, new TestLogger()));

            $"Then the version is '1.0.0-alpha.0'"
                .x(() => Assert.Equal("1.0.0-alpha.0", actualVersion.ToString()));
        }
    }
}
