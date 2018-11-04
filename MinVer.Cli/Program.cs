namespace MinVer.Cli
{
    using System;
    using McMaster.Extensions.CommandLineUtils;

    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            var buildMetadata = app.Option("-b|--build-metadata <BUILD_METADATA>", "The build metadata to append to the version.", CommandOptionType.SingleValue);
            var minimumMajorMinor = app.Option("-m|--minimum-major-minor <MINIMUM_MAJOR_MINOR>", "The minimum major and minor version range. E.g. '2.0'.", CommandOptionType.SingleValue);
            var path = app.Option("-p|--path <PATH>", "The path of the repository.", CommandOptionType.SingleValue);
            var tagPrefix = app.Option("-t|--tag-prefix <TAG_PREFIX>", "The tag prefix.", CommandOptionType.SingleValue);
            var verbose = app.Option("-v|--verbose", "Enable verbose logging.", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                var minimumMajor = 0;
                var minimumMinor = 0;

                var minimumMajorMinorValue = minimumMajorMinor.Value();

                if (!string.IsNullOrEmpty(minimumMajorMinorValue))
                {
                    var numbers = minimumMajorMinorValue.Split('.');

                    if (numbers.Length > 2)
                    {
                        throw new Exception($"More than one dot in minimum major and minor version range '{minimumMajorMinorValue}'.");
                    }

                    if (!int.TryParse(numbers[0], out minimumMajor))
                    {
                        throw new Exception($"Invalid major version '{numbers[0]}' in minimum major and minor version range '{minimumMajorMinorValue}'.");
                    }

                    if (numbers.Length > 1 && !int.TryParse(numbers[1], out minimumMinor))
                    {
                        throw new Exception($"Invalid minor version '{numbers[1]}' in minimum major and minor version range '{minimumMajorMinorValue}'.");
                    }
                }

                Console.Out.WriteLine(Versioner.GetVersion(path.Value() ?? ".", verbose.HasValue(), tagPrefix.Value(), minimumMajor, minimumMinor, buildMetadata.Value()));
            });

            app.Execute(args);
        }
    }
}
