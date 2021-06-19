using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace MinVerTests.Infra
{
    public static class Git
    {
        public static async Task EnsureEmptyRepositoryAndCommit(string path)
        {
            await EnsureEmptyRepository(path).ConfigureAwait(false);
            await Commit(path).ConfigureAwait(false);
        }

        public static Task Commit(string path, Action<string> log = null) =>
            Cli.Wrap("git").WithArguments("commit -m '.' --allow-empty").WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log);

        public static async Task EnsureEmptyRepository(string path)
        {
            FileSystem.EnsureEmptyDirectory(path);
            await Init(path).ConfigureAwait(false);
        }

        public static async Task Init(string path, Action<string> log = null)
        {
            _ = await Cli.Wrap("git").WithArguments("init --initial-branch=main").WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);
            _ = await Cli.Wrap("git").WithArguments("config user.email johndoe@tempuri.org").WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);
            _ = await Cli.Wrap("git").WithArguments("config user.name John Doe").WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);
            _ = await Cli.Wrap("git").WithArguments("config commit.gpgsign false").WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false);
        }

        public static async Task<string> GetGraph(string path, Action<string> log = null) =>
            (await Cli.Wrap("git").WithArguments("log --graph --pretty=format:'%d'")
                .WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log).ConfigureAwait(false))
            .StandardOutput;

        public static Task Tag(string path, string tag, Action<string> log = null) =>
            Cli.Wrap("git").WithArguments($"tag {tag}").WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log);

        public static Task Tag(string path, string tagName, string sha, Action<string> log = null) =>
            Cli.Wrap("git").WithArguments($"tag {tagName} {sha}").WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log);

        public static Task AnnotatedTag(string path, string tag, string message, Action<string> log = null) =>
            Cli.Wrap("git").WithArguments($"tag {tag} -a -m '{message}'").WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log);

        public static async Task<IEnumerable<string>> GetCommitShas(string path, Action<string> log = null)
        {
            var stdOutLines = new List<string>();

            _ = await Cli.Wrap("git")
                .WithArguments("log --pretty=format:\"%H\"")
                .WithWorkingDirectory(path)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(stdOutLines.Add))
                .ExecuteBufferedLoggedAsync(log)
                .ConfigureAwait(false);

            return stdOutLines;
        }

        public static Task Checkout(string path, string sha, Action<string> log = null) =>
            Cli.Wrap("git").WithArguments($"checkout {sha}").WithWorkingDirectory(path).ExecuteBufferedLoggedAsync(log);
    }
}
