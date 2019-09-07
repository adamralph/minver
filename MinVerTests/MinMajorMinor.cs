namespace MinVerTests
{
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
            $"Given an empty git repository in '{path = GetScenarioDirectory("minimum-major-minor-not-tagged")}'"
                .x(() => EnsureEmptyRepository(path));

            "When the version is determined using minimum major minor '1.2'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, new MajorMinor(1, 2), default, default, default, new TestLogger()));

            "Then the version is '1.2.0-alpha.0'"
                .x(() => Assert.Equal("1.2.0-alpha.0", actualVersion.ToString()));
        }

        [Scenario]
        [Example("2.0.0", 1, 0, "2.0.0", true)]
        [Example("2.0.0", 2, 0, "2.0.0", true)]
        [Example("2.0.0", 3, 0, "3.0.0-alpha.0", false)]
        public static void Tagged(string tag, int major, int minor, string expectedVersion, bool isRedundant, string path, TestLogger logger, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"minimum-major-minor-tagged-{tag}-{major}-{minor}")}'"
                .x(() => EnsureEmptyRepositoryAndCommit(path));

            $"And the commit is tagged '{tag}'"
                .x(() => Tag(path, tag));

            $"When the version is determined using minimum major minor '{major}.{minor}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, new MajorMinor(major, minor), default, default, default, logger = new TestLogger()));

            $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));

            if (isRedundant)
            {
                "And a debug message is logged because the minimum major minor is redundant"
                    .x(() => Assert.Contains(logger.DebugMessages, message => message.Contains($"The calculated version {actualVersion} satisfies the minimum major minor {major}.{minor}.")));
            }
        }

        [Scenario]
        public static void NotTagged(string path, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory("minimum-major-minor-not-tagged")}'"
                .x(() => EnsureEmptyRepositoryAndCommit(path));

            "When the version is determined using minimum major minor '1.0'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, new MajorMinor(1, 0), default, default, default, new TestLogger()));

            "Then the version is '1.0.0-alpha.0'"
                .x(() => Assert.Equal("1.0.0-alpha.0", actualVersion.ToString()));
        }
    }
}
