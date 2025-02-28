using System.Diagnostics.CodeAnalysis;

namespace MinVer.Lib;

internal static class Git
{
    private static readonly char[] newLineChars = ['\r', '\n',];

    public static bool IsWorkingDirectory(string directory, ILogger log) => GitCommand.TryRun("status --short", directory, log, out _);

    public static bool TryGetHead(string directory, [NotNullWhen(returnValue: true)] out Commit? head, ILogger log)
    {
        head = null;

        if (!GitCommand.TryRun("log --pretty=format:\"%H %P\"", directory, log, out var output))
        {
            return false;
        }

        var lines = output.Split(newLineChars, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            return false;
        }

        var commits = new Dictionary<string, Commit>();

        foreach (var shas in lines
            .Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries)))
        {
            commits.GetOrAdd(shas[0], () => new Commit(shas[0]))
                .Parents.AddRange(shas.Skip(1).Select(parentSha => commits.GetOrAdd(parentSha, () => new Commit(parentSha))));
        }

        head = commits.Values.First();

        return true;
    }

    public static IEnumerable<(string Name, string Sha)> GetTags(string directory, ILogger log) =>
        GitCommand.TryRun("show-ref --tags --dereference", directory, log, out var output)
            ? output
                .Split(newLineChars, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(" ", 2))
                .Select(tokens => (tokens[1][10..].RemoveFromEnd("^{}"), tokens[0]))
            : [];

    public static bool TryGetCurrentBranch(string directory, [NotNullWhen(returnValue: true)] out string? branchName, ILogger log)
    {
        branchName = null;

        if (!GitCommand.TryRun("rev-parse --abbrev-ref HEAD", directory, log, out var output))
        {
            _ = log.IsDebugEnabled && log.Debug("Failed to get current branch name.");
            return false;
        }

        branchName = output.Trim();

        if (string.IsNullOrEmpty(branchName) || branchName == "HEAD")
        {
            _ = log.IsDebugEnabled && log.Debug("Current branch name is empty or HEAD - likely detached HEAD state.");
            return false;
        }

        _ = log.IsDebugEnabled && log.Debug($"Current branch name is '{branchName}'");
        return true;
    }

    private static string RemoveFromEnd(this string text, string value) =>
        text.EndsWith(value, StringComparison.OrdinalIgnoreCase) ? text[..^value.Length] : text;
}
