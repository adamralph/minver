using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MinVerTests.Infra.FileSystem;
using static SimpleExec.Command;

namespace MinVerTests.Infra
{
    public static class Git
    {
        public static void EnsureEmptyRepositoryAndCommit(string path)
        {
            EnsureEmptyRepository(path);
            Commit(path);
        }

        public static void Commit(string path) => Run("git", "commit -m '.' --allow-empty", path);

        public static void EnsureEmptyRepository(string path)
        {
            EnsureEmptyDirectory(path);
            Init(path);
            PrepareForCommits(path);
        }

        public static void Init(string path) => Run("git", "init", path);

        public static void PrepareForCommits(string path)
        {
            Run("git", "config user.email johndoe@tempuri.org", path);
            Run("git", "config user.name John Doe", path);
            Run("git", "config commit.gpgsign false", path);
        }

        public static Task<string> GetGraph(string path) => ReadAsync("git", "log --graph --pretty=format:'%d'", path);

        internal static void Tag(string path, string tag) => Run("git", $"tag {tag}", path);

        internal static void Tag(string path, string tagName, string sha) => Run("git", $"tag {tagName} {sha}", path);

        internal static void AnnotatedTag(string path, string tag, string message) => Run("git", $"tag {tag} -a -m '{message}'", path);

        internal static IEnumerable<string> GetCommitShas(string path) =>
            Read("git", "log --pretty=format:\"%H\"", path, noEcho: true)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        internal static void Checkout(string path, string sha) => Run("git", $"checkout {sha}", path);
    }
}
