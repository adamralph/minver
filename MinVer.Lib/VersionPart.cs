namespace MinVer.Lib
{
    public enum VersionPart
    {
        Patch = 0,
        Minor = 1,
        Major = 2,
    }

    public static class VersionPartExtensions
    {
        public static string ValidValues => "major, minor, or patch";

        public static string Default => "patch";
    }
}
