namespace MinVer.Lib;

public enum VersionPart
{
    Revision = 0,
    Patch = 1,
    Minor = 2,
    Major = 3,
}

public static class VersionPartExtensions
{
    public static string ValidValues => "major, minor, patch or revision (default)";
}
