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

    public static class AutoIncrement
    {
        [Scenario]
        [Example("1.2.3", VersionPart.Major, "2.0.0-alpha.0.1")]
        [Example("1.2.3", VersionPart.Minor, "1.3.0-alpha.0.1")]
        [Example("1.2.3", VersionPart.Patch, "1.2.4-alpha.0.1")]
        public static void RtmVersionIncrement(string tag, VersionPart autoIncrement, string expectedVersion, string path, Repository repo, Version actualVersion)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory($"rtm-auto-increment-{tag}")}'"
                .x(c => repo = EnsureEmptyRepositoryAndCommit(path).Using(c));

            $"And the commit is tagged '{tag}'"
                .x(() => repo.ApplyTag(tag));

            $"And another commit"
                .x(() => Commit(path));

            $"When the version is determined using auto-increment '{autoIncrement}'"
                .x(() => actualVersion = Versioner.GetVersion(path, default, default, default, autoIncrement, new TestLogger()));

            $"Then the version is '{expectedVersion}'"
                .x(() => Assert.Equal(expectedVersion, actualVersion.ToString()));
        }
    }
}
