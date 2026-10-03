namespace Fixtures;

public static class Git
{
    public static async Task EnsureEmptyRepositoryAndCommit(string path)
    {
        await EnsureEmptyRepository(path).ConfigureAwait(false);
        await Commit(path).ConfigureAwait(false);
    }

    public static Task Commit(string path) =>
        LoggingCommand.ReadAsync("git", "commit --message='.' --allow-empty", path);

    public static Task EnsureEmptyRepository(string path)
    {
        FileSystem.EnsureEmptyDirectory(path);
        return Init(path);
    }

    public static async Task Init(string path)
    {
        _ = await LoggingCommand.ReadAsync("git", "init --initial-branch=main", path).ConfigureAwait(false);
        _ = await LoggingCommand.ReadAsync("git", "config user.email johndoe@tempuri.org", path).ConfigureAwait(false);
        _ = await LoggingCommand.ReadAsync("git", "config user.name John Doe", path).ConfigureAwait(false);
        _ = await LoggingCommand.ReadAsync("git", "config commit.gpgsign false", path).ConfigureAwait(false);
    }

    public static async Task<string> GetGraph(string path) =>
        (await LoggingCommand.ReadAsync("git", "log --graph --pretty=format:'%d'", path).ConfigureAwait(false)).StandardOutput;

    public static Task Tag(string path, string tag) =>
        LoggingCommand.ReadAsync("git", $"tag {tag}", path);

    public static Task Tag(string path, string tag, string sha) =>
        LoggingCommand.ReadAsync("git", $"tag {tag} {sha}", path);

    public static Task AnnotatedTag(string path, string tag, string message) =>
        LoggingCommand.ReadAsync("git", $"tag {tag} --annotate --message='{message}'", path);

    public static async Task<IReadOnlyCollection<string>> GetCommitShas(string path) =>
        (await LoggingCommand.ReadAsync("git", "log --pretty=format:\"%H\"", path).ConfigureAwait(false)).StandardOutput.Split('\r', '\n');

    public static Task SwitchToBranch(string path, string branch) =>
        LoggingCommand.ReadAsync("git", $"switch {branch}", path);

    public static Task SwitchToCommit(string path, string sha) =>
        LoggingCommand.ReadAsync("git", $"switch {sha} --detach", path);
}
