namespace MinVerTests
{
    using System.IO;
    using MinVer;
    using Xbehave;
    using Xunit;
    using static MinVerTests.Infra.Git;
    using static MinVerTests.Infra.FileSystem;
    using static SimpleExec.Command;

    public static class MSBuildIntegration
    {
        [Scenario]
        public static void Subdirectory(string path, Version version)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory("msbuild-integration-subdirectory")}'"
                .x(async () => await EnsureRepositoryWithACommit(path));

            "And the commit is tagged 2.0.0"
                .x(async () => await RunAsync("git", "tag 2.0.0", path));

            "And the repository has a subdirectory"
                .x(() => EnsureEmptyDirectory(path = Path.Combine(path, "subdirectory")));

            "When the version is determined using the subdirectory"
                .x(() => version = Versioner.GetVersion(path, default, default, default, default));

            "Then the version is 2.0.0"
                .x(() => Assert.Equal("2.0.0", version.ToString()));
        }
    }
}
