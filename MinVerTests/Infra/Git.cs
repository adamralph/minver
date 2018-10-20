namespace MinVerTests.Infra
{
    using System.Threading.Tasks;
    using static MinVerTests.Infra.FileSystem;
    using static SimpleExec.Command;

    public static class Git
    {
        public static async Task EnsureEmptyRepository(string path)
        {
            EnsureDirectoryDeleted(path);
            EnsureDirectoryCreated(path);

            await RunAsync("git", "init", path);
        }
    }
}
