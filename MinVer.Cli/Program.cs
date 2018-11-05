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
            var majorMinor = app.Option("-m|--major-minor <MAJOR.MINOR>", "The MAJOR.MINOR version range. E.g. '2.0'.", CommandOptionType.SingleValue);
            var path = app.Option("-p|--path <PATH>", "The path of the repository.", CommandOptionType.SingleValue);
            var tagPrefix = app.Option("-t|--tag-prefix <TAG_PREFIX>", "The tag prefix.", CommandOptionType.SingleValue);
            var verbose = app.Option<bool>("-v|--verbose <VERBOSE>", "Verbose logging.", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var major = 0;
                var minor = 0;

                var majorMinorValue = majorMinor.Value();

                if (!string.IsNullOrEmpty(majorMinorValue))
                {
                    var numbers = majorMinorValue.Split('.');

                    if (numbers.Length > 2)
                    {
                        throw new Exception($"More than one dot in MAJOR.MINOR range '{majorMinorValue}'.");
                    }

                    if (!int.TryParse(numbers[0], out major))
                    {
                        throw new Exception($"Invalid MAJOR '{numbers[0]}' in MAJOR.MINOR range '{majorMinorValue}'.");
                    }

                    if (numbers.Length > 1 && !int.TryParse(numbers[1], out minor))
                    {
                        throw new Exception($"Invalid MINOR '{numbers[1]}' in MAJOR.MINOR range '{majorMinorValue}'.");
                    }
                }

                Console.Out.WriteLine(Versioner.GetVersion(path.Value() ?? ".", verbose.ParsedValue, tagPrefix.Value(), major, minor, buildMetadata.Value()));
            });

            app.Execute(args);
        }
    }
}
