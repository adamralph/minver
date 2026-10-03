using System.Reflection;
using System.Text;
using Fixtures;
using Testing;
using Xunit;
using static SimpleExec.Command;

namespace Tests.Packages;

public static class LogMessages
{
    [Fact]
    public static async Task MinVer()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);
        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "v2.3.4-alpha-x.5");
        var envVars = new (string, string)[]
        {
            ("MinVerAutoIncrement", "minor"),
            ("MinVerBuildMetadata", "build.6"),
            ("MinVerDefaultPreReleaseIdentifiers", "preview.0"),
            ("MinVerIgnoreHeight", "true"),
            ("MinVerMinimumMajorMinor", "1.0"),
            ("MinVerTagPrefix", "v"),
        };

        // act
        var (_, standardOutput, _) = await DotNetCli.BuildProject(path, envVars: envVars);

        // assert
        var lines = new StringBuilder();
        foreach (var line in standardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.TrimStart().StartsWith("MinVer:", StringComparison.Ordinal))
            {
                _ = lines.AppendLine(line);
            }
        }

        var messages = lines.ToString();
        messages = messages.Replace(GetPhysicalPath(path), "{path}", StringComparison.Ordinal);
        messages = await ReplaceShas(messages, Solution.GetFullPath("."));
        messages = await ReplaceShas(messages, path);
        await messages.Verify();
    }

    [Fact]
    public static async Task MinVerCli()
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory();
        await DotNetCli.CreateProject(path);
        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4-alpha-x.5+build.6");

        // act
        var (_, standardError) = await Testing.MinVerCli.ReadAsync(path);

        // assert
        standardError = await ReplaceShas(standardError, Solution.GetFullPath("."));
        standardError = await ReplaceShas(standardError, path);
        await standardError.Verify();
    }

    private static string GetPhysicalPath(string path)
    {
        var full = Path.GetFullPath(path);
        var root = Path.GetPathRoot(full)!;
        var physical = root;

        foreach (var part in Path.GetRelativePath(root, full).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = new DirectoryInfo(Path.Combine(physical, part));
            physical = candidate.LinkTarget is null
                ? candidate.FullName
                : Path.GetFullPath(candidate.ResolveLinkTarget(true)!.FullName);
        }

        return physical;
    }

    private static async Task<string> ReplaceShas(this string logMessages, string path)
    {
        var shas = (await ReadAsync("git", "log --pretty=format:\"%H\"", path))
            .StandardOutput
            .ToNonEmptyLines()
            .Reverse()
            .ToList();

        foreach (var sha in shas)
        {
            logMessages = logMessages.Replace(sha, "{sha}", StringComparison.Ordinal);
        }

        foreach (var shortSha in shas.Select(sha => sha[..7]))
        {
            logMessages = logMessages.Replace(shortSha, "{sha}", StringComparison.Ordinal);
        }

        return logMessages;
    }
}
