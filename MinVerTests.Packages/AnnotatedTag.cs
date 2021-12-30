using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class AnnotatedTag
    {
        [Fact]
        public static async Task HasTagVersion()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.AnnotatedTag(path, "2.3.4-alpha.5+build.6", "foo");

            var expected = Package.WithVersion(2, 3, 4, new[] { "alpha", "5" }, 0, "build.6");

            // act
            var (actual, _, _) = await Sdk.BuildProject(path);
            var (cliStandardOutput, _) = await MinVerCli.ReadAsync(path);

            // assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected.Version, cliStandardOutput.Trim());
        }
    }
}
