using System.Reflection;
using System.Threading.Tasks;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;

namespace MinVerTests.Lib
{
    public static class TagPrefixes
    {
        [Theory]
        [InlineData("2.3.4", "", "2.3.4")]
        [InlineData("v3.4.5", "v", "3.4.5")]
        [InlineData("version5.6.7", "version", "5.6.7")]
        public static async Task TagPrefix(string tag, string prefix, string expectedVersion)
        {
            // act
            var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, prefix));
            await EnsureEmptyRepositoryAndCommit(path);
            await Tag(path, tag);

            // act
            var actualVersion = Versioner.GetVersion(path, prefix, MajorMinor.Zero, "", default, "", NullLogger.Instance);

            // assert
            Assert.Equal(expectedVersion, actualVersion.ToString());
        }
    }
}
