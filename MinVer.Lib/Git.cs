namespace MinVer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Git
    {
        public static bool IsWorkingDirectory(string directory, ILogger log) => GitCommand.TryRun("status --short", directory, log, out _);

        public static Commit GetHeadOrDefault(string directory, ILogger log)
        {
            if (!GitCommand.TryRun("log --pretty=format:\"%H %P\"", directory, log, out var output))
            {
                return null;
            }

            string[] subtreeRoots = Array.Empty<string>();
            if (GitCommand.TryRun("rev-list --max-parents=0 --grep=git-subtree-dir HEAD", directory, log, out var subtreeOutput))
            {
                subtreeRoots = subtreeOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }

            var commits = new Dictionary<string, Commit>();

            foreach (var shas in output
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)))
            {
                commits.GetOrAdd(shas[0], () => new Commit(shas[0], subtreeRoots.Contains(shas[0])))
                    .Parents.AddRange(shas.Skip(1).Select(parentSha => commits.GetOrAdd(parentSha, () => new Commit(parentSha, subtreeRoots.Contains(parentSha)))));
            }

            return commits.Values.FirstOrDefault();
        }

        public static IEnumerable<Tag> GetTagsOrEmpty(string directory, ILogger log) =>
            GitCommand.TryRun("show-ref --tags --dereference", directory, log, out var output)
                ? output
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split(new[] { ' ' }, 2))
                    .Select(tokens => new Tag(tokens[1].Substring(10).RemoveFromEnd("^{}"), tokens[0]))
                : Enumerable.Empty<Tag>();

        private static string RemoveFromEnd(this string text, string value) =>
            text.EndsWith(value, StringComparison.OrdinalIgnoreCase) ? text.Substring(0, text.Length - value.Length) : text;
    }
}
