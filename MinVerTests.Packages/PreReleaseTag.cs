using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class PreReleaseTag
    {
        [Fact]
        public static async Task HasTagVersion()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.Tag(path, "2.3.4-alpha.5");

            var expected = Package.WithVersion(2, 3, 4, new[] { "alpha", "5" });

            // act
            var (actual, _) = await Sdk.BuildProject(path);
            var cli = await MinVerCli.ReadAsync(path);

            // assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected.Version, cli.StandardOutput.Trim());
        }
    }
}
