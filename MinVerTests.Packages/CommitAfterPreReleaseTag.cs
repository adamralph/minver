using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class CommitAfterPreReleaseTag
    {
        [Fact]
        public static async Task HasHeightOne()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.Tag(path, "2.3.4-alpha.5");
            await Git.Commit(path);

            var expected = Package.WithVersion(2, 3, 4, new[] { "alpha", "5" }, 1);

            // act
            var (sdkActual, _) = await Sdk.BuildProject(path);
            var (cliActual, _) = await MinVerCli.Run(path);

            // assert
            Assert.Equal(expected, sdkActual);
            Assert.Equal(expected.Version, cliActual);
        }
    }
}
