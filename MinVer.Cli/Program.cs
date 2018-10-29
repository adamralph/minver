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

            Console.WriteLine(Versioner.GetVersion(args[0], args.ElementAtOrDefault(1)));
        }
    }
}
