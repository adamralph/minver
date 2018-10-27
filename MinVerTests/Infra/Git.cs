namespace MinVerTests.Infra
{
    using System.Threading.Tasks;
    using static MinVerTests.Infra.FileSystem;
    using static SimpleExec.Command;

    public static class Git
    {
        public static async Task EnsureRepositoryWithACommit(string path)
        {
            await EnsureEmptyRepository(path);

            await RunAsync("git", @"config user.email 'johndoe @tempuri.org'", path);
            await RunAsync("git", @"config user.name 'John Doe'", path);
            await RunAsync("git", @"config commit.gpgsign false", path);
            await RunAsync("git", @"commit --allow-empty -m '.'", path);
        }

        public static async Task EnsureEmptyRepository(string path)
        {
            EnsureEmptyDirectory(path);

            await RunAsync("git", "init", path);
        }
    }
}
