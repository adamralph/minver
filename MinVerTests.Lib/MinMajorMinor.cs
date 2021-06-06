using System;
using System.Reflection;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xbehave;
using Xunit;
using static MinVerTests.Infra.Git;
using Version = MinVer.Lib.Version;

namespace MinVerTests.Lib
{
    public static class MinMajorMinor
    {
        [Scenario]
        public static void NoCommits(string path, Version actualVersion)
        {
            _ = $"Given an empty git repository in {path = MethodBase.GetCurrentMethod().GetTestDirectory()}"
                .x(() => EnsureEmptyRepository(path));

            _ = "When the version is determined using minimum major minor '1.2'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, new MajorMinor(1, 2), default, default, default, default));

            _ = "Then the version is '1.2.0-alpha.0'"
                .x(() => Assert.Equal("1.2.0-alpha.0", actualVersion.ToString()));
        }

        [Scenario]
        [Example("4.0.0", 3, 2, "4.0.0", true)]
        [Example("4.3.0", 4, 3, "4.3.0", true)]
        [Example("4.3.0", 5, 4, "5.4.0-alpha.0", false)]
        public static void Tagged(string tag, int major, int minor, string expectedVersion, bool isRedundant, string path, TestLogger logger, Version actualVersion)
        {
            _ = $"Given a git repository with a commit in {path = MethodBase.GetCurrentMethod().GetTestDirectory((major, minor))}"
                .x(() => EnsureEmptyRepositoryAndCommit(path));

            _ = $"And the commit is tagged '{tag}'"
                .x(() => Tag(path, tag));

            _ = $"When the version is determined using minimum major minor '{major}.{minor}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, new MajorMinor(major, minor), default, default, default, logger = new TestLogger()));

            _ = $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));

            if (isRedundant)
            {
                _ = "And a debug message is logged because the minimum major minor is redundant"
                    .x(() => Assert.Contains(logger.DebugMessages, message => message.Contains($"The calculated version {actualVersion} satisfies the minimum major minor {major}.{minor}.", StringComparison.Ordinal)));
            }
        }

        [Scenario]
        public static void NotTagged(string path, Version actualVersion)
        {
            _ = $"Given a git repository with a commit in {path = MethodBase.GetCurrentMethod().GetTestDirectory()}"
                .x(() => EnsureEmptyRepositoryAndCommit(path));

            _ = "When the version is determined using minimum major minor '1.0'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, new MajorMinor(1, 0), default, default, default, default));

            _ = "Then the version is '1.0.0-alpha.0'"
                .x(() => Assert.Equal("1.0.0-alpha.0", actualVersion.ToString()));
        }
    }
}
