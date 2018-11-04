namespace MinVerTests
{
    using MinVer;
    using Xbehave;
    using Xunit;
    using static MinVerTests.Infra.Git;
    using static MinVerTests.Infra.FileSystem;
    using static SimpleExec.Command;

    public static class BuildMetadata
    {
        [Scenario]
        [Example("1.2.3+a", default, "1.2.3+a")]
        [Example("1.2.3", "b", "1.2.3+b")]
        [Example("1.2.3+a", "b", "1.2.3+a.b")]
        [Example("1.2.3-pre+a", default, "1.2.3-pre+a")]
        [Example("1.2.3-pre", "b", "1.2.3-pre+b")]
        [Example("1.2.3-pre+a", "b", "1.2.3-pre+a.b")]
        public static void CurrentTag(string tag, string buildMetadata, string expectedVersion, string path, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"build-metadata-current-tag-{tag}-{buildMetadata}")}'"
                .x(async () => await EnsureRepositoryWithACommit(path));

            $"And the commit is tagged '{tag}'"
                .x(async () => await RunAsync("git", $"tag {tag}", path));

            $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, default, default, default, buildMetadata));

            $"Then the version is '{expectedVersion}'"
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
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"build-metadata-current-tag-{tag}-{buildMetadata}")}'"
                .x(async () => await EnsureRepositoryWithACommit(path));

            $"And the commit is tagged '{tag}'"
                .x(async () => await RunAsync("git", $"tag {tag}", path));

            $"And another commit"
                .x(async () => await Commit(path));

            $"When the version is determined using build metadata '{buildMetadata}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, default, default, default, buildMetadata));

            $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }
    }
}
