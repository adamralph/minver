namespace MinVer.Lib;

internal static class Git
{
    private static readonly char[] NewLineChars = ['\r', '\n',];

    public static async Task<bool> IsWorkingDirectoryAsync(string directory, ILogger log) =>
        await GitCommand.TryRunAsync("status --porcelain", directory, log) is not null;

    public static async Task<Commit?> TryGetHeadAsync(string directory, ILogger log)
    {
        if (await GitCommand.TryRunAsync("log --pretty=format:\"%H %P\"", directory, log) is not { } output)
        {
            return null;
        }

        var lines = output.Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            return null;
        }

        var commits = new Dictionary<string, Commit>();

        foreach (var shas in lines
            .Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries)))
        {
            commits.GetOrAdd(shas[0], () => new Commit(shas[0]))
                .Parents.AddRange(shas.Skip(1).Select(parentSha => commits.GetOrAdd(parentSha, () => new Commit(parentSha))));
        }

        return commits.Values.First();
    }

    public static async Task<IEnumerable<(string Name, string Sha)>> GetTagsAsync(string directory, ILogger log) =>
        await GitCommand.TryRunAsync("show-ref --tags --dereference", directory, log) is { } output
            ? output
                .Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(" ", 2))
                .Select(tokens => (tokens[1][10..].RemoveFromEnd("^{}"), tokens[0]))
            : [];

    private static string RemoveFromEnd(this string text, string value) =>
        text.EndsWith(value, StringComparison.OrdinalIgnoreCase) ? text[..^value.Length] : text;
}
