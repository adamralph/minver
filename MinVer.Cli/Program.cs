namespace MinVer.Cli
{
    using System;
    using McMaster.Extensions.CommandLineUtils;

    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            var buildMetadata = app.Option("-b|--build-metadata <BUILD_METADATA>", "The build metadata to append to the version.", CommandOptionType.SingleValue);
            var majorMinor = app.Option("-m|--major-minor <MAJOR.MINOR>", "The MAJOR.MINOR version range. E.g. '2.0'.", CommandOptionType.SingleValue);
            var path = app.Option("-p|--path <PATH>", "The path of the repository.", CommandOptionType.SingleValue);
            var tagPrefix = app.Option("-t|--tag-prefix <TAG_PREFIX>", "The tag prefix.", CommandOptionType.SingleValue);
            var verbose = app.Option("-v|--verbose", "Enable verbose logging.", CommandOptionType.NoValue);

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
                        Console.Out.WriteLine($"MinVer: error MINVER0004 : More than one dot in MAJOR.MINOR range '{majorMinorValue}'.");
                        return 2;
                    }

                    if (!int.TryParse(numbers[0], out major))
                    {
                        Console.Out.WriteLine($"MinVer: error MINVER0005 : Invalid MAJOR '{numbers[0]}' in MAJOR.MINOR range '{majorMinorValue}'.");
                        return 2;
                    }

                    if (numbers.Length > 1 && !int.TryParse(numbers[1], out minor))
                    {
                        Console.Out.WriteLine($"MinVer: error MINVER0006 : Invalid MINOR '{numbers[1]}' in MAJOR.MINOR range '{majorMinorValue}'.");
                        return 2;
                    }
                }

                Console.Out.WriteLine(Versioner.GetVersion(path.Value() ?? ".", verbose.HasValue(), tagPrefix.Value(), major, minor, buildMetadata.Value()));
                return 0;
            });

            return app.Execute(args);
        }
    }
}
