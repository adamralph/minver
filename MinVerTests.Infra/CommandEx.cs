using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimpleExec;

namespace MinVerTests.Infra
{
    internal static class CommandEx
    {
#if NET5_0_OR_GREATER
        private static readonly ConcurrentDictionary<string, int> indices = new();
#else
        private static readonly ConcurrentDictionary<string, int> indices = new ConcurrentDictionary<string, int>();
#endif

        public static async Task<Result> ReadLoggedAsync(string name, string args = "", string workingDirectory = "", IEnumerable<KeyValuePair<string, string>>? envVars = null)
        {
            envVars = (envVars ?? Enumerable.Empty<KeyValuePair<string, string>>()).ToList();

            var result = await Command.ReadAsync(
                name,
                args,
                workingDirectory,
                configureEnvironment: env =>
                {
                    foreach (var pair in envVars)
                    {
                        env[pair.Key] = pair.Value;
                    }
                }).ConfigureAwait(false);

            var index = 0;

            lock (indices)
            {
                index = indices.GetOrAdd(workingDirectory, 0);
                indices[workingDirectory] = index + 1;
            }

            var markdown =
$@"
# Command read {index}

## Command

### Name

`{name}`

### Args

`{args}`

### Working directory

`{workingDirectory}`

### Environment variables

```text
{string.Join(Environment.NewLine, envVars.Select(pair => $"{pair.Key}={pair.Value}"))}
```

## Result

### StandardOutput (stdout)

```text
{result.StandardOutput}
```

### StandardError (stderr)

```text
{result.StandardError}
```
";

            var markdownFileName = Path.Combine(workingDirectory, $"command-read-{index:D2}.md");

            await File.WriteAllTextAsync(markdownFileName, markdown).ConfigureAwait(false);

            return result;
        }
    }
}
