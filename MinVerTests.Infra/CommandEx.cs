using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleExec;

namespace MinVerTests.Infra
{
    internal static class CommandEx
    {
        private static int index;

        public static async Task<Result> ReadLoggedAsync(string name, string args = null, string workingDirectory = null, IEnumerable<KeyValuePair<string, string>> envVars = null)
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

            var index = Interlocked.Increment(ref CommandEx.index);

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
