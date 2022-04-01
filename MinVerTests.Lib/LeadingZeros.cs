using System.Reflection;
using System.Threading.Tasks;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;

namespace MinVerTests.Lib
{
    public static class TagWithLeadingZeros
    {
        [Theory]
        [InlineData("02.03.04", "2.3.4")]
        [InlineData("2.3.4-beta.05", "2.3.4-beta.5")]
        [InlineData("2.3.4-beta.5.0a", "2.3.4-beta.5.0a")]
        [InlineData("2.3.4+build.05", "2.3.4+build.05")]
        [InlineData("02.03.04-beta.05+build.06", "2.3.4-beta.5+build.06")]
        [InlineData("002.003.004", "2.3.4")]
        [InlineData("2.3.4-beta.005", "2.3.4-beta.5")]
        [InlineData("2.3.4-beta.5.00a", "2.3.4-beta.5.00a")]
        [InlineData("2.3.4+build.005", "2.3.4+build.005")]
        [InlineData("002.003.004-beta.005+build.006", "2.3.4-beta.5+build.006")]
        public static async Task TagPrefix(string tag, string expectedVersion)
        {
            // act
            var path = MethodBase.GetCurrentMethod().GetTestDirectory(tag);
            await EnsureEmptyRepositoryAndCommit(path);
            await Tag(path, tag);

            // act
            var actualVersion = Versioner.GetVersion(path, "", MajorMinor.Zero, "", default, "", NullLogger.Instance, ignoreLeadingZeros: true);

            // assert
            Assert.Equal(expectedVersion, actualVersion.ToString());
        }
    }
}
