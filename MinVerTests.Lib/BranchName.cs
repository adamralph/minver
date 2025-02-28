using MinVer.Lib;
using MinVerTests.Lib.Infra;
using Xunit;
using Version = MinVer.Lib.Version;

namespace MinVerTests.Lib;

public static class BranchName
{
    [Fact]
    public static void IncludesBranchName()
    {
        // arrange
        var defaultVersionIdentifiers = new[] { "alpha", "0" };

        // Create a test instance of Version
        var version = new Version(0, 0, 0, [.. defaultVersionIdentifiers], 0, "");

        // act
        version = Versioner.AppendBranchName(version, "develop", NullLogger.Instance);

        // assert
        var versionString = version.ToString();
        Assert.Equal("0.0.0-develop.alpha.0", versionString);
    }

    [Fact]
    public static void VersionWithoutPreReleaseWithBranchName()
    {
        // arrange
        var version = new Version(2, 0, 0, [], 0, "");

        // act
        version = Versioner.AppendBranchName(version, "feature/foo", NullLogger.Instance);

        // assert
        var versionString = version.ToString();
        Assert.Equal("2.0.0-feature_foo", versionString);  // Note the / is replaced with -
    }

    [Fact]
    public static void VersionWithHeightWithBranchName()
    {
        // arrange
        var defaultVersionIdentifiers = new[] { "alpha", "0" };
        var version = new Version(1, 2, 3, [.. defaultVersionIdentifiers], 42, "");

        // act
        version = Versioner.AppendBranchName(version, "bugfix/123", NullLogger.Instance);

        // assert
        var versionString = version.ToString();
        Assert.Equal("1.2.3-bugfix_123.alpha.0.42", versionString);  // Height is added at the end
    }
}
