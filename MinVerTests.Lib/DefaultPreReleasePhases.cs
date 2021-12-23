using System.Reflection;
using System.Threading.Tasks;
using MinVer.Lib;
using MinVerTests.Infra;
using MinVerTests.Lib.Infra;
using Xunit;
using static MinVerTests.Infra.Git;

namespace MinVerTests.Lib
{
    public static class DefaultPreReleasePhases
    {
        [Theory]
        [InlineData("", "0.0.0-alpha.0")]
        [InlineData("preview", "0.0.0-preview.0")]
        public static async Task DefaultPreReleasePhase(string phase, string expectedVersion)
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory(phase);
            await EnsureEmptyRepositoryAndCommit(path);

            // act
            var actualVersion = Versioner.GetVersion(path, "", MajorMinor.Zero, "", default, phase, NullLogger.Instance);

            // assert
            Assert.Equal(expectedVersion, actualVersion.ToString());
        }
    }
}
