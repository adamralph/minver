namespace MinVer
{
    using System.IO;
    using MinVer.Lib;

    internal static class TextWriterExtensions
    {
        public static void WriteVersion(this TextWriter writer, Version version)
        {
            writer.WriteLine($"Version:{version}");
            writer.WriteLine($"VersionPrefix:{version.ToPrefix()}");
            writer.WriteLine($"VersionSuffix:{version.ToSuffix()}");
            writer.WriteLine($"Major:{version.Major}");
            writer.WriteLine($"Minor:{version.Minor}");
            writer.WriteLine($"Patch:{version.Patch}");
        }
    }
}
