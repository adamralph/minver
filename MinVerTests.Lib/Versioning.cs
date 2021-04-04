using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CliWrap;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xbehave;
using Xunit;
using static MinVerTests.Infra.FileSystem;
using static MinVerTests.Infra.Git;
using Version = MinVer.Lib.Version;

namespace MinVerTests.Lib
{
    public static class Versioning
    {
#if NET5_0_OR_GREATER
        private static readonly Dictionary<string, string> historicalCommands = new()
#else
        private static readonly Dictionary<string, string> historicalCommands = new Dictionary<string, string>
#endif
        {
            {
                "general",
@"git commit --allow-empty -m '.'
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
git tag 1.1.0 -a -m '.'"
            }
        };

        [Scenario]
        [Example("general")]
        public static void RepoWithHistory(string name, string path)
        {
            $"Given a git repository with a history of branches and/or tags in {path = MethodBase.GetCurrentMethod().GetTestDirectory(name)}"
                .x(async () =>
                {
                    await EnsureEmptyRepositoryAndCommit(path);

                    foreach (var command in historicalCommands[name].Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var nameAndArgs = command.Split(" ", 2);
                        _ = await Cli.Wrap(nameAndArgs[0]).WithArguments(nameAndArgs[1]).WithWorkingDirectory(path).ExecuteAsync();
                        await Task.Delay(200);
                    }
                });

            "When the version is determined for every commit"
                .x(async () =>
                {
                    var versionCounts = new Dictionary<string, int>();
                    foreach (var sha in await GetCommitShas(path))
                    {
                        await Checkout(path, sha);

                        var version = Versioner.GetVersion(path, default, default, default, default, default, default);
                        var versionString = version.ToString();
                        var tagName = $"v/{versionString}";

                        _ = versionCounts.TryGetValue(versionString, out var oldVersionCount);
                        var versionCount = oldVersionCount + 1;
                        versionCounts[versionString] = versionCount;

                        tagName = versionCount > 1
                            ? $"v({versionCount})/{versionString}"
                            : tagName;

                        await Tag(path, tagName, sha);
                    }

                    await Checkout(path, "master");
                });

            "Then the versions are as expected"
                .x(async () => await AssertFile.Contains($"../../../{name}.txt", await GetGraph(path)));
        }

        [Scenario]
        public static void EmptyRepo(string path, Version version)
        {
            $"Given an empty git repository in {path = MethodBase.GetCurrentMethod().GetTestDirectory()}"
                .x(() => EnsureEmptyRepository(path));

            "When the version is determined"
                .x(() => version = Versioner.GetVersion(path, default, default, default, default, default, default));

            "Then the version is 0.0.0-alpha.0"
                .x(() => Assert.Equal("0.0.0-alpha.0", version.ToString()));
        }

        [Scenario]
        public static void NoRepo(string path, Version version)
        {
            $"Given an empty directory {path = MethodBase.GetCurrentMethod().GetTestDirectory()}"
                .x(() => EnsureEmptyDirectory(path));

            "When the version is determined"
                .x(() => version = Versioner.GetVersion(path, default, default, default, default, default, default));

            "Then the version is 0.0.0-alpha.0"
                .x(() => Assert.Equal("0.0.0-alpha.0", version.ToString()));
        }
    }
}
