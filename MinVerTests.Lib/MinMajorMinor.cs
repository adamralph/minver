using System;
using System.Reflection;
using System.Threading.Tasks;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;

namespace MinVerTests.Lib
{
    public static class MinMajorMinor
    {
        [Fact]
        public static async Task NoCommits()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await EnsureEmptyRepository(path);

            // act
            var actualVersion = Versioner.GetVersion(path, "", new MajorMinor(1, 2), "", default, "", NullLogger.Instance);

            // assert
            Assert.Equal("1.2.0-alpha.0", actualVersion.ToString());
        }

        [Theory]
        [InlineData("4.0.0", 3, 2, "4.0.0", true)]
        [InlineData("4.3.0", 4, 3, "4.3.0", true)]
        [InlineData("4.3.0", 5, 4, "5.4.0-alpha.0", false)]
        public static async Task Tagged(string tag, int major, int minor, string expectedVersion, bool isRedundant)
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, major, minor));
            await EnsureEmptyRepositoryAndCommit(path);
            await Tag(path, tag);
            var logger = new TestLogger();

            // act
            var actualVersion = Versioner.GetVersion(path, "", new MajorMinor(major, minor), "", default, "", logger);

            // assert
            Assert.Equal(expectedVersion, actualVersion.ToString());

            if (isRedundant)
            {
                Assert.Contains(logger.Messages, message => message.Text.Contains($"The calculated version {actualVersion} satisfies the minimum major minor {major}.{minor}.", StringComparison.Ordinal));
            }
        }

        [Fact]
        public static async Task NotTagged()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await EnsureEmptyRepositoryAndCommit(path);

            // act
            var actualVersion = Versioner.GetVersion(path, "", new MajorMinor(1, 0), "", default, "", NullLogger.Instance);

            // assert
            Assert.Equal("1.0.0-alpha.0", actualVersion.ToString());
        }
    }
}
