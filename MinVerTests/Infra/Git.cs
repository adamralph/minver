namespace MinVerTests.Infra
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using static MinVerTests.Infra.FileSystem;
    using static SimpleExec.Command;

    public static class Git
    {
        public static void EnsureEmptyRepositoryAndCommit(string path)
        {
            EnsureEmptyRepository(path);
            Commit(path);
        }

        public static void Commit(string path) => Read("git", "commit -m '.' --allow-empty", path);

        public static void EnsureEmptyRepository(string path)
        {
            EnsureEmptyDirectory(path);
            Init(path);
            PrepareForCommits(path);
        }

        public static void Init(string path) => Read("git", "init", path);

        public static void PrepareForCommits(string path)
        {
            Read("git", "config user.email johndoe@tempuri.org", path);
            Read("git", "config user.name John Doe", path);
            Read("git", "config commit.gpgsign false", path);
        }

        public static Task<string> GetGraph(string path) => ReadAsync("git", "log --graph --pretty=format:'%d'", path);

        internal static void Tag(string path, string tag) => Read("git", $"tag {tag}", path);

        internal static void Tag(string path, string tagName, string sha) => Read("git", $"tag {tagName} {sha}", path);

        internal static void AnnotatedTag(string path, string tag, string message) => Read("git", $"tag {tag} -a -m '{message}'", path);

        internal static IEnumerable<string> GetCommitShas(string path) =>
            Read("git", "log --pretty=format:\"%H\"", path, noEcho: true)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        internal static void Checkout(string path, string sha) => Read("git", $"checkout {sha}", path);
    }
}
