using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinVer.Lib;
using MinVerTests.Lib.Infra;
using Xbehave;
using Xunit;
using static MinVerTests.Lib.Infra.FileSystem;
using static MinVerTests.Lib.Infra.Git;
using static SimpleExec.Command;
using Version = MinVer.Lib.Version;

namespace MinVerTests.Lib
{
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
git tag 1.1.0 -a -m '.'
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
                    EnsureEmptyRepositoryAndCommit(path);

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
                    foreach (var sha in GetCommitShas(path))
                    {
                        Checkout(path, sha);

                        var version = Versioner.GetVersion(path, default, default, default, default, default, default);
                        var versionString = version.ToString();
                        var tagName = $"v/{versionString}";

                        versionCounts.TryGetValue(versionString, out var oldVersionCount);
                        var versionCount = oldVersionCount + 1;
                        versionCounts[versionString] = versionCount;

                        tagName = versionCount > 1
                            ? $"v({versionCount})/{versionString}"
                            : tagName;

                        Tag(path, tagName, sha);
                    }

                    Checkout(path, "master");
                });

            "Then the versions are as expected"
                .x(async () => await AssertFile.Contains($"../../../{name}.txt", await GetGraph(path)));
        }

        [Scenario]
        public static void EmptyRepo(string path, Version version)
        {
            $"Given an empty git repository in '{path = GetScenarioDirectory("versioning-empty-repo")}'"
                .x(() => EnsureEmptyRepository(path));

            "When the version is determined"
                .x(() => version = Versioner.GetVersion(path, default, default, default, default, default, default));

            "Then the version is 0.0.0-alpha.0"
                .x(() => Assert.Equal("0.0.0-alpha.0", version.ToString()));
        }

        [Scenario]
        public static void NoRepo(string path, Version version)
        {
            $"Given an empty directory '{path = GetScenarioDirectory("versioning-no-repo")}'"
                .x(() => EnsureEmptyDirectory(path));

            "When the version is determined"
                .x(() => version = Versioner.GetVersion(path, default, default, default, default, default, default));

            "Then the version is 0.0.0-alpha.0"
                .x(() => Assert.Equal("0.0.0-alpha.0", version.ToString()));
        }
    }
}
