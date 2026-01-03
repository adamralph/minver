namespace MinVerTests.Infra;

public static class Git
{
    public static async Task EnsureEmptyRepositoryAndCommit(string path, Ct ct)
    {
        await EnsureEmptyRepository(path, ct).ConfigureAwait(false);
        await Commit(path, ct).ConfigureAwait(false);
    }

    public static Task Commit(string path, Ct ct) =>
        CommandEx.ReadLoggedAsync("git", ct, "commit --message='.' --allow-empty", path);

    public static Task EnsureEmptyRepository(string path, Ct ct)
    {
        FileSystem.EnsureEmptyDirectory(path);
        return Init(path, ct);
    }

    public static async Task Init(string path, Ct ct)
    {
        _ = await CommandEx.ReadLoggedAsync("git", ct, "init --initial-branch=main", path).ConfigureAwait(false);
        _ = await CommandEx.ReadLoggedAsync("git", ct, "config user.email johndoe@tempuri.org", path).ConfigureAwait(false);
        _ = await CommandEx.ReadLoggedAsync("git", ct, "config user.name John Doe", path).ConfigureAwait(false);
        _ = await CommandEx.ReadLoggedAsync("git", ct, "config commit.gpgsign false", path).ConfigureAwait(false);
    }

    public static async Task<string> GetGraph(string path, Ct ct) =>
        (await CommandEx.ReadLoggedAsync("git", ct, "log --graph --pretty=format:'%d'", path).ConfigureAwait(false)).StandardOutput;

    public static Task Tag(string path, string tag, Ct ct) =>
        CommandEx.ReadLoggedAsync("git", ct, $"tag {tag}", path);

    public static Task Tag(string path, string tag, string sha, Ct ct) =>
        CommandEx.ReadLoggedAsync("git", ct, $"tag {tag} {sha}", path);

    public static Task AnnotatedTag(string path, string tag, string message, Ct ct) =>
        CommandEx.ReadLoggedAsync("git", ct, $"tag {tag} --annotate --message='{message}'", path);

    public static async Task<IReadOnlyCollection<string>> GetCommitShas(string path, Ct ct) =>
        (await CommandEx.ReadLoggedAsync("git", ct, "log --pretty=format:\"%H\"", path).ConfigureAwait(false)).StandardOutput.Split('\r', '\n');

    public static Task SwitchToBranch(string path, string branch, Ct ct) =>
        CommandEx.ReadLoggedAsync("git", ct, $"switch {branch}", path);

    public static Task SwitchToCommit(string path, string sha, Ct ct) =>
        CommandEx.ReadLoggedAsync("git", ct, $"switch {sha} --detach", path);
}
