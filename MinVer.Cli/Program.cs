namespace MinVer.Cli
{
    using System;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("Path not specified.");
            }

            var verbose = false;
            var verboseString = args.ElementAtOrDefault(1);
            if (!string.IsNullOrEmpty(verboseString) && !bool.TryParse(verboseString, out verbose))
            {
                throw new Exception($"MinVer verbose string '{verboseString}' cannot be converted to a Boolean value.");
            }

            Console.WriteLine(Versioner.GetVersion(args[0], verbose, args.ElementAtOrDefault(2)));
        }
    }
}
