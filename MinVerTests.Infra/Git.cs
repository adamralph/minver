namespace MinVerTests.Infra;

public static class Git
{
    public static async Task EnsureEmptyRepositoryAndCommit(string path)
    {
        await EnsureEmptyRepository(path).ConfigureAwait(false);
        await Commit(path).ConfigureAwait(false);
    }

    public static Task Commit(string path) =>
        CommandEx.ReadLoggedAsync("git", "commit -m '.' --allow-empty", path);

    public static Task EnsureEmptyRepository(string path)
    {
        FileSystem.EnsureEmptyDirectory(path);
        return Init(path);
    }

    public static async Task Init(string path)
    {
        _ = await CommandEx.ReadLoggedAsync("git", "init --initial-branch=main", path).ConfigureAwait(false);
        _ = await CommandEx.ReadLoggedAsync("git", "config user.email johndoe@tempuri.org", path).ConfigureAwait(false);
        _ = await CommandEx.ReadLoggedAsync("git", "config user.name John Doe", path).ConfigureAwait(false);
        _ = await CommandEx.ReadLoggedAsync("git", "config commit.gpgsign false", path).ConfigureAwait(false);
    }

    public static async Task<string> GetGraph(string path) =>
        (await CommandEx.ReadLoggedAsync("git", "log --graph --pretty=format:'%d'", path).ConfigureAwait(false)).StandardOutput;

    public static Task Tag(string path, string tag) =>
        CommandEx.ReadLoggedAsync("git", $"tag {tag}", path);

    public static Task Tag(string path, string tag, string sha) =>
        CommandEx.ReadLoggedAsync("git", $"tag {tag} {sha}", path);

    public static Task AnnotatedTag(string path, string tag, string message) =>
        CommandEx.ReadLoggedAsync("git", $"tag {tag} -a -m '{message}'", path);

    public static async Task<IEnumerable<string>> GetCommitShas(string path) =>
        (await CommandEx.ReadLoggedAsync("git", "log --pretty=format:\"%H\"", path).ConfigureAwait(false)).StandardOutput.Split('\r', '\n');

    public static Task Checkout(string path, string sha) =>
        CommandEx.ReadLoggedAsync("git", $"checkout {sha}", path);
}
