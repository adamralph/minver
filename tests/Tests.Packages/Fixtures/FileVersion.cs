namespace Tests.Packages.Fixtures;

public sealed record FileVersion(int FileMajorPart, int FileMinorPart, int FileBuildPart, int FilePrivatePart, string ProductVersion);
