using System.Reflection;
using System.Threading.Tasks;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;

namespace MinVerTests.Lib
{
    public static class AutoIncrement
    {
        [Theory]
        [InlineData("1.2.3", VersionPart.Major, "2.0.0-alpha.0.1")]
        [InlineData("1.2.3", VersionPart.Minor, "1.3.0-alpha.0.1")]
        [InlineData("1.2.3", VersionPart.Patch, "1.2.4-alpha.0.1")]
        public static async Task RtmVersionIncrement(string tag, VersionPart autoIncrement, string expectedVersion)
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory((tag, autoIncrement));
            await EnsureEmptyRepositoryAndCommit(path);
            await Tag(path, tag);
            await Commit(path);

            // act
            var actualVersion = Versioner.GetVersion(path, "", MajorMinor.Zero, "", autoIncrement, "", NullLogger.Instance);

            // assert
            Assert.Equal(expectedVersion, actualVersion.ToString());
        }
    }
}
