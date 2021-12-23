using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MinVer.Lib
{
    internal static class Git
    {
        public static bool IsWorkingDirectory(string directory, ILogger log) => GitCommand.TryRun("status --short", directory, log, out _);

        public static bool TryGetHead(string directory, [NotNullWhen(returnValue: true)] out Commit? head, ILogger log)
        {
            head = null;

            if (!GitCommand.TryRun("log --pretty=format:\"%H %P\"", directory, log, out var output))
            {
                return false;
            }

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
            {
                return false;
            }

            var commits = new Dictionary<string, Commit>();

            foreach (var shas in lines
                .Select<string, string[]>(line => line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)))
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
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split(new[] { ' ' }, 2))
                    .Select(tokens => (tokens[1][10..].RemoveFromEnd("^{}"), tokens[0]))
                : Enumerable.Empty<(string, string)>();

        private static string RemoveFromEnd(this string text, string value) =>
            text.EndsWith(value, StringComparison.OrdinalIgnoreCase) ? text[..^value.Length] : text;
    }
}
