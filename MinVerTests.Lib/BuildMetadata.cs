using System.Reflection;
using MinVer.Lib;
using MinVerTests.Infra;
using Xbehave;
using Xunit;
using static MinVerTests.Infra.Git;
using Version = MinVer.Lib.Version;

namespace MinVerTests.Lib
{
    public static class BuildMetadata
    {
        [Scenario]
        [Example(default, "0.0.0-alpha.0")]
        [Example("a", "0.0.0-alpha.0+a")]
        public static void NoCommits(string buildMetadata, string expectedVersion, string path, Version actualVersion)
        {
            _ = $"Given an empty git repository in {path = MethodBase.GetCurrentMethod().GetTestDirectory(buildMetadata)}"
                .x(() => EnsureEmptyRepository(path));

            _ = $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, default, buildMetadata, default, default, default));

            _ = $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }

        [Scenario]
        [Example(default, "0.0.0-alpha.0")]
        [Example("a", "0.0.0-alpha.0+a")]
        public static void NoTag(string buildMetadata, string expectedVersion, string path, Version actualVersion)
        {
            _ = $"Given a git repository with a commit in {path = MethodBase.GetCurrentMethod().GetTestDirectory(buildMetadata)}"
                .x(() => EnsureEmptyRepositoryAndCommit(path));

            _ = $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, default, buildMetadata, default, default, default));

            _ = $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }

        [Scenario]
        [Example("1.2.3+a", default, "1.2.3+a")]
        [Example("1.2.3", "b", "1.2.3+b")]
        [Example("1.2.3+a", "b", "1.2.3+a.b")]
        [Example("1.2.3-pre+a", default, "1.2.3-pre+a")]
        [Example("1.2.3-pre", "b", "1.2.3-pre+b")]
        [Example("1.2.3-pre+a", "b", "1.2.3-pre+a.b")]
        public static void CurrentTag(string tag, string buildMetadata, string expectedVersion, string path, Version actualVersion)
        {
            _ = $"Given a git repository with a commit in {path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, buildMetadata))}"
                .x(() => EnsureEmptyRepositoryAndCommit(path));

            _ = $"And the commit is tagged '{tag}'"
                .x(() => Tag(path, tag));

            _ = $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, default, buildMetadata, default, default, default));

            _ = $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }

        [Scenario]
        [Example("1.2.3+a", default, "1.2.4-alpha.0.1")]
        [Example("1.2.3", "b", "1.2.4-alpha.0.1+b")]
        [Example("1.2.3+a", "b", "1.2.4-alpha.0.1+b")]
        [Example("1.2.3-pre+a", default, "1.2.3-pre.1")]
        [Example("1.2.3-pre", "b", "1.2.3-pre.1+b")]
        [Example("1.2.3-pre+a", "b", "1.2.3-pre.1+b")]
        public static void PreviousTag(string tag, string buildMetadata, string expectedVersion, string path, Version actualVersion)
        {
            _ = $"Given a git repository with a commit in {path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, buildMetadata))}"
                .x(() => EnsureEmptyRepositoryAndCommit(path));

            _ = $"And the commit is tagged '{tag}'"
                .x(() => Tag(path, tag));

            _ = "And another commit"
                .x(() => Commit(path));

            _ = $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, default, buildMetadata, default, default, default));

            _ = $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }
    }
}
