namespace MinVerTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using LibGit2Sharp;
    using MinVer;
    using MinVerTests.Infra;
    using Xbehave;
    using Xunit;
    using static MinVerTests.Infra.Git;
    using static SimpleExec.Command;

    public static class Versioning
    {
        [Scenario]
        [Example(
            "general",
            @"
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 0.0.0-alpha.1
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 0.0.0
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 0.1.0-beta.1
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 0.1.0
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.0.0-alpha.1
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.0.0-alpha.2
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.0.0-beta.1
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.0.0-beta.2
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.0.0-rc.1
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.0.0-rc.2
git tag 1.0.0
git checkout -b foo
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.1.0-alpha.1
git checkout master
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.1.0-alpha.2
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git merge foo --no-edit
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.1.0-beta.1
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.1.0-beta.2
git tag 1.1.0-beta.10
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git commit --allow-empty -m "".""
git tag 1.1.0-rc.1
git tag 1.1.0
")]
        public static void RepoWithHistory(string name, string historicalCommands, string path)
        {
            $"Given a git repository in `{path = Path.Combine(Path.GetTempPath(), name)}` with a history of branches and/or tags"
                .x(async () =>
                {
                    await EnsureEmptyRepository(path);

                    await RunAsync("git", @"config user.email ""johndoe @tempuri.org""", path);
                    await RunAsync("git", @"config user.name ""John Doe""", path);
                    await RunAsync("git", @"config commit.gpgsign false", path);
                    await RunAsync("git", @"commit --allow-empty -m "".""", path);

                    foreach (var command in historicalCommands.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
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

                            var version = Versioner.GetVersion(path);
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
        public static void EmptyRepo(string name, string path, MinVer.Version version)
        {
            $"Given an empty repo git repository in `{path = Path.Combine(Path.GetTempPath(), name = "empty-repo")}`"
                .x(async () => await EnsureEmptyRepository(path));

            "When the version is determined"
                .x(() => version = Versioner.GetVersion(path));

            "Then the version is 0.0.0-alpha.0"
                .x(() => Assert.Equal("0.0.0-alpha.0", version.ToString()));
        }
    }
}
