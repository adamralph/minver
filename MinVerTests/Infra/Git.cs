namespace MinVerTests.Infra
{
    using System.Threading.Tasks;
    using LibGit2Sharp;

    using static MinVerTests.Infra.FileSystem;
    using static SimpleExec.Command;

    public static class Git
    {
        public static Repository EnsureEmptyRepositoryAndCommit(string path)
        {
            var repo = EnsureEmptyRepository(path);

            Commit(path);

            return repo;
        }

        public static void Commit(string path) => Run("git", "commit -m '.' --allow-empty", path);

        public static Repository EnsureEmptyRepository(string path)
        {
            EnsureEmptyDirectory(path);

            Repository.Init(path);

            return new Repository(path).PrepareForCommits();
        }

        public static Repository PrepareForCommits(this Repository repo)
        {
            repo.Config.Set("user.email", "johndoe @tempuri.org");
            repo.Config.Set("user.name", "John Doe");
            repo.Config.Set("commit.gpgsign", "false");

            return repo;
        }

        public static Task<string> GetGraph(string path) => ReadAsync("git", "log --graph --pretty=format:'%d'", path);
    }
}
