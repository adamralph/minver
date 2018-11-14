namespace MinVerTests
{
    using System.IO;
    using LibGit2Sharp;
    using MinVer;
    using Xbehave;
    using Xunit;
    using static MinVerTests.Infra.FileSystem;
    using static MinVerTests.Infra.Git;
    using static SimpleExec.Command;

    public static class MSBuildIntegration
    {
        [Scenario]
        public static void Subdirectory(string path, string subdirectory, bool isRepositoryCreated, Repository repository)
        {
            $"Given a git repository with a commit in '{path = GetScenarioDirectory("msbuild-integration-subdirectory")}'"
                .x(async () => await EnsureRepositoryWithACommit(path));

            "And the commit is tagged 2.0.0"
                .x(async () => await RunAsync("git", "tag 2.0.0", path));

            "And the repository has a subdirectory"
                .x(() => EnsureEmptyDirectory(subdirectory = Path.Combine(path, "subdirectory")));

            "When an attempt is made to create a repository object using the subdirectory"
                .x(() => isRepositoryCreated = RepositoryEx.TryCreateRepo(subdirectory, out repository));

            "Then the repository is created"
                .x(() => Assert.True(isRepositoryCreated));

            "And the repository object has the same working directory as the repository"
                .x(() => Assert.Equal(path, repository.Info.WorkingDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)));
        }
    }
}
