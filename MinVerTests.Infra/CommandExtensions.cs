using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace MinVerTests.Infra
{
    internal static class CommandExtensions
    {
        private static int index;

        public static async Task<BufferedCommandResult> ExecuteBufferedLoggedAsync(this Command command)
        {
            var validation = command.Validation;

            var result = await command.WithValidation(CommandResultValidation.None).ExecuteBufferedAsync();

            var index = Interlocked.Increment(ref CommandExtensions.index);

            var log =
$@"
# Command {index}

## Target file path

`{command.TargetFilePath}`

## Arguments

`{command.Arguments}`

## Environment variables

```text
{string.Join(Environment.NewLine, command.EnvironmentVariables.Select(pair => $"{pair.Key}={pair.Value}"))}
```

## Exit code

`{result.ExitCode}`

## Standard error

```text
{result.StandardError}
```

## Standard output

```text
{result.StandardOutput}
```
";

            await File.WriteAllTextAsync(Path.Combine(command.WorkingDirPath, $"command-{index:D2}.md"), log);

            return result.ExitCode == 0 || validation == CommandResultValidation.None ? result : throw new Exception(log);
        }
    }
}
