using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;
using static SimpleExec.Command;

namespace MinVerTests.Lib
{
    public static class LogMessages
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 0)]
        public static async Task RepoWithHistory(int minMajor, int minMinor)
        {
            // arrange
            var minMajorMinor = new MajorMinor(minMajor, minMinor);

            var historicalCommands =
                @"
git commit --allow-empty -m '.'
git tag not-a-version
git checkout -b foo
git commit --allow-empty -m '.'
git tag 1.0.0-foo.1
git checkout main
git merge foo --no-edit --no-ff
git checkout -b bar
git commit --allow-empty -m '.'
git checkout main
git checkout -b baz
git commit --allow-empty -m '.'
git checkout main
git merge bar baz --no-edit --no-ff --strategy=octopus
";

            var path = MethodBase.GetCurrentMethod().GetTestDirectory(minMajorMinor);

            await EnsureEmptyRepository(path);

            foreach (var item in historicalCommands
                .Split(new[] { '\r', '\n', }, StringSplitOptions.RemoveEmptyEntries)
                .Select((command, index) => new { Command = command, Index = $"{index}", }))
            {
                if (item.Command.StartsWith("git commit", StringComparison.Ordinal))
                {
                    // Sometimes git seems to treat bar and baz as a single branch if the commits are empty.
                    // This probably occurs during the octopus merge.
                    // So let's add a file before each commit to ensure that doesn't happen.
                    await File.WriteAllTextAsync(Path.Combine(path, item.Index), item.Index);
                    _ = await ReadAsync("git", $"add {item.Index}", path);

                    // if not enough delay is given between commits,
                    // the order of parallel commits on different branches seems to be non-deterministic
                    await Task.Delay(1100);
                }

                var nameAndArgs = item.Command.Split(" ", 2);
                _ = await ReadAsync(nameAndArgs[0], nameAndArgs[1], path);
            }

            var log = new TestLogger();

            // act
            _ = Versioner.GetVersion(path, "", minMajorMinor, "", default, "", log);

            // assert
            var logMessages = log.ToString();

            var shas = (await ReadAsync("git", "log --pretty=format:\"%H\"", path))
                .StandardOutput
                .Split(new[] { '\r', '\n', }, StringSplitOptions.RemoveEmptyEntries)
                .Reverse()
                .ToList();

            foreach (var item in shas.Select((sha, index) => new { Sha = sha, Index = index, }))
            {
                logMessages = logMessages.Replace(item.Sha, $"{item.Index}", StringComparison.Ordinal);
            }

            foreach (var item in shas.Select((sha, index) => new { ShortSha = sha[..7], Index = index, }))
            {
                logMessages = logMessages.Replace(item.ShortSha, $"{item.Index}", StringComparison.Ordinal);
            }

            await AssertFile.Contains($"../../../log.{minMajorMinor}.txt", logMessages);
        }
    }
}
