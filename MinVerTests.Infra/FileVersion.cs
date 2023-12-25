namespace MinVerTests.Infra;

public record FileVersion(int FileMajorPart, int FileMinorPart, int FileBuildPart, int FilePrivatePart, string ProductVersion);
