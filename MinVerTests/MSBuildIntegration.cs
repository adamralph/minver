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

            "And the git repository has a subdirectory"
                .x(() => EnsureEmptyDirectory(path = Path.Combine(path, "subdirectory")));

            "And the current commit is tagged 1.0.0"
                .x(async () => await RunAsync("git", @"tag 1.0.0", path));

            "When the version is determined using the subdirectory"
                .x(() => version = Versioner.GetVersion(path));

            "Then the version is 1.0.0"
                .x(() => Assert.Equal("1.0.0", version.ToString()));
        }
    }
}
