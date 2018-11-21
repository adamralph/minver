namespace MinVer
{
    using System.IO;
    using MinVer.Lib;

    internal static class TextWriterExtensions
    {
        public static void WriteVersion(this TextWriter writer, Version version) => writer.WriteLine(version);
    }
}
