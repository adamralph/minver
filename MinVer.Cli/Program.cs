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

            var path = app.Option("-p|--path <PATH>", "The path of the repository.", CommandOptionType.SingleValue);
            var tagPrefix = app.Option("-t|--tag-prefix <TAG-PREFIX>", "The tag prefix.", CommandOptionType.SingleValue);
            var verbose = app.Option("-v|--verbose", "Enable verbose logging.", CommandOptionType.NoValue);

            app.OnExecute(() => Console.WriteLine(Versioner.GetVersion(path.Value() ?? ".", verbose.HasValue(), tagPrefix.Value())));

            app.Execute(args);
        }
    }
}
