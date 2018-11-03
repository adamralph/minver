namespace MinVerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LibGit2Sharp;
    using MinVer;
    using MinVerTests.Infra;
    using Xbehave;
    using Xunit;
    using static MinVerTests.Infra.Git;
    using static MinVerTests.Infra.FileSystem;
    using static SimpleExec.Command;

    public static class Versioning
    {
        private static readonly Dictionary<string, string> historicalCommands = new Dictionary<string, string>
        {
            {
                "general",
@"
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 0.0.0-alpha.1
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 0.0.0
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 0.1.0-beta.1
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 0.1.0
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 1.0.0-alpha.1
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 1.0.0-rc.1
git tag 1.0.0
git checkout -b foo
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 1.0.1-alpha.1
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 1.0.1
git commit --allow-empty -m '.'
git checkout master
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 1.1.0-alpha.1
git commit --allow-empty -m '.'
git merge foo --no-edit
git commit --allow-empty -m '.'
git tag 1.1.0-beta.2
git tag 1.1.0-beta.10
git commit --allow-empty -m '.'
git commit --allow-empty -m '.'
git tag 1.1.0-rc.1
git tag 1.1.0
"
            }
        };

        [Scenario]
        [Example("general")]
        public static void RepoWithHistory(string name, string path)
        {
            $"Given a git repository in '{path = GetScenarioDirectory("versioning-repo-with-history-" + name)}' with a history of branches and/or tags"
                .x(async () =>
                {
                    await EnsureRepositoryWithACommit(path);

                    foreach (var command in historicalCommands[name].Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var nameAndArgs = command.Split(" ", 2);
                        await RunAsync(nameAndArgs[0], nameAndArgs[1], path);
                        await Task.Delay(200);
                    }
                });

            "When the version is determined for every commit"
                .x(() =>
                {
                    var versionCounts = new Dictionary<string, int>();

                    using (var repo = new Repository(path))
                    {
                        foreach (var commit in repo.Commits)
                        {
                            Commands.Checkout(repo, commit);

                            var version = Versioner.GetVersion(path, default, default, default, default, default);
                            var versionString = version.ToString();
                            var tagName = $"v/{versionString}";

                            if (!repo.Tags.Any(tag => tag.Target.Sha == commit.Sha && tag.FriendlyName == tagName))
                            {
                                versionCounts.TryGetValue(versionString, out var oldVersionCount);
                                var versionCount = oldVersionCount + 1;
                                versionCounts[versionString] = versionCount;

                                tagName = versionCount > 1
                                    ? $"v({versionCount})/{versionString}"
                                    : tagName;

                                repo.Tags.Add(tagName, commit);
                            }
                        }

                        Commands.Checkout(repo, repo.Branches["master"]);
                    }
                });

            "Then the versions are as expected"
                .x(async () => await AssertFile.Contains($"../../../{name}.txt", await ReadAsync("git", "log --graph --pretty=format:'%d'", path)));
        }

        [Scenario]
        public static void EmptyRepo(string path, MinVer.Version version)
        {
            $"Given an empty git repository in '{path = GetScenarioDirectory("versioning-empty-repo")}'"
                .x(async () => await EnsureEmptyRepository(path));

            "When the version is determined"
                .x(() => version = Versioner.GetVersion(path, default, default, default, default, default));

            "Then the version is 0.0.0-alpha.0"
                .x(() => Assert.Equal("0.0.0-alpha.0", version.ToString()));
        }

        [Scenario]
        public static void NoRepo(string path, MinVer.Version version)
        {
            $"Given an empty directory '{path = GetScenarioDirectory("versioning-no-repo")}'"
                .x(() => EnsureEmptyDirectory(path));

            "When the version is determined"
                .x(() => version = Versioner.GetVersion(path, default, default, default, default, default));

            "Then the version is 0.0.0-alpha.0"
                .x(() => Assert.Equal("0.0.0-alpha.0", version.ToString()));
        }

        [Scenario]
        public static void NoDirectory(string path, Exception ex)
        {
            "Given a non-existent directory"
                .x(() => path = Guid.NewGuid().ToString());

            "When the version is determined"
                .x(() => ex = Record.Exception(() => Versioner.GetVersion(path, default, default, default, default, default)));

            "Then an exception is thrown"
                .x(() => Assert.NotNull(ex));

            "And the exception message contains the path"
                .x(() => Assert.Contains(path, ex.Message));
        }
    }
}
