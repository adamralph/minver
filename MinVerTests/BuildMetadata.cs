using System.Linq;

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

    public static class BuildMetadata
    {
        [Scenario]
        [Example(default, default, "0.0.0-alpha.0")]
        [Example("a", default, "0.0.0-alpha.0+a")]
        [Example(default, "pre", "0.0.0-pre")]
        [Example("a", "pre", "0.0.0-pre+a")]
        public static void NoCommits(string buildMetadata, string defaultPrereleaseIdentifiers, string expectedVersion, string path, Repository repo, Version actualVersion)
        {
            $"Given an empty git repository in '{path = GetScenarioDirectory($"build-metadata-no-tag-{buildMetadata}")}'"
                .x(c => repo = EnsureEmptyRepository(path).Using(c));

            $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(repo, default, default, buildMetadata, new VersionSettings(defaultPrereleaseIdentifiers?.Split(".")), new TestLogger()));

            $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }

        [Scenario]
        [Example(default, default, "0.0.0-alpha.0")]
        [Example("a", default, "0.0.0-alpha.0+a")]
        [Example(default, "pre", "0.0.0-pre")]
        [Example("a", "pre", "0.0.0-pre+a")]
        public static void NoTag(string buildMetadata, string defaultPrereleaseIdentifiers, string expectedVersion, string path, Repository repo, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"build-metadata-no-tag-{buildMetadata}")}'"
                .x(c => repo = EnsureEmptyRepositoryAndCommit(path).Using(c));

            $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(repo, default, default, buildMetadata, new VersionSettings(defaultPrereleaseIdentifiers?.Split(".")), new TestLogger()));

            $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }

        [Scenario]
        [Example("1.2.3+a", default, "1.2.3+a")]
        [Example("1.2.3", "b", "1.2.3+b")]
        [Example("1.2.3+a", "b", "1.2.3+a.b")]
        [Example("1.2.3-pre+a", default, "1.2.3-pre+a")]
        [Example("1.2.3-pre", "b", "1.2.3-pre+b")]
        [Example("1.2.3-pre+a", "b", "1.2.3-pre+a.b")]
        public static void CurrentTag(string tag, string buildMetadata, string expectedVersion, string path, Repository repo, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"build-metadata-current-tag-{tag}-{buildMetadata}")}'"
                .x(c => repo = EnsureEmptyRepositoryAndCommit(path).Using(c));

            $"And the commit is tagged '{tag}'"
                .x(() => repo.ApplyTag(tag));

            $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(repo, default, default, buildMetadata, default, new TestLogger()));

            $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }

        [Scenario]
        [Example("1.2.3+a", default, default, "1.2.4-alpha.0.1")]
        [Example("1.2.3+a", default, "pre", "1.2.4-pre.1")]
        [Example("1.2.3", "b", default, "1.2.4-alpha.0.1+b")]
        [Example("1.2.3", "b", "pre", "1.2.4-pre.1+b")]
        [Example("1.2.3+a", "b", default, "1.2.4-alpha.0.1+b")]
        [Example("1.2.3+a", "b", "pre", "1.2.4-pre.1+b")]
        [Example("1.2.3-pre+a", default, default, "1.2.3-pre.1")]
        [Example("1.2.3-pre", "b", default, "1.2.3-pre.1+b")]
        [Example("1.2.3-pre+a", "b", default, "1.2.3-pre.1+b")]
        public static void PreviousTag(string tag, string buildMetadata, string defaultPrereleaseIdentifiers, string expectedVersion, string path, Repository repo, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"build-metadata-previous-tag-{tag}-{buildMetadata}")}'"
                .x(c => repo = EnsureEmptyRepositoryAndCommit(path).Using(c));

            $"And the commit is tagged '{tag}'"
                .x(() => repo.ApplyTag(tag));

            $"And another commit"
                .x(() => Commit(path));

            $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(repo, default, default, buildMetadata, new VersionSettings(defaultPrereleaseIdentifiers?.Split(".")), new TestLogger()));

            $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }
    }
}
