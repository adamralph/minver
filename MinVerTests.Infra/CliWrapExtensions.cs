using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.Builders;
using CliWrap.Exceptions;

namespace MinVerTests.Infra
{
    internal static class CliWrapExtensions
    {
        private static int index;

        public static async Task<BufferedCommandResult> ExecuteBufferedLoggedAsync(this Command command, Action<string> log)
        {
            if (log != null)
            {
                command = command.WithStandardErrorPipe(PipeTarget.ToDelegate(line => log($"stderr: {line}")));
                command = command.WithStandardOutputPipe(PipeTarget.ToDelegate(line => log($"stdout: {line}")));
            }

            var validation = command.Validation;

            log?.Invoke($"Running command: {command}");
            var result = await command.WithValidation(CommandResultValidation.None).ExecuteBufferedAsync();
            log?.Invoke($"Finished running command: {command}");

            var index = Interlocked.Increment(ref CliWrapExtensions.index);

            var markdown =
$@"
# Command {index}

## Target file path

`{command.TargetFilePath}`

## Arguments

`{command.Arguments}`

## Working directory path

`{command.WorkingDirPath}`

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

            var markdownFileName = Path.Combine(command.WorkingDirPath, $"command-{index:D2}.md");

            log?.Invoke($"Writing markdown to '{markdownFileName}'...");
            await File.WriteAllTextAsync(markdownFileName, markdown).ConfigureAwait(false);
            log?.Invoke($"Finished writing markdown to '{markdownFileName}'");

            return result.ExitCode == 0 || validation == CommandResultValidation.None ? result : throw new CommandExecutionException(command, result.ExitCode, markdown);
        }

        public static EnvironmentVariablesBuilder SetFrom(this EnvironmentVariablesBuilder env, IEnumerable<KeyValuePair<string, string>> source)
        {
            foreach (var (key, value) in source)
            {
                env = env.Set(key, value);
            }

            return env;
        }

        public static ArgumentsBuilder AddIf(this ArgumentsBuilder args, bool condition, string value)
        {
            if (condition)
            {
                args = args.Add(value);
            }

            return args;
        }
    }
}
