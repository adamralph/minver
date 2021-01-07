using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class BuildMetadata
    {
        [Fact]
        public static async Task HasBuildMetadata()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);
            var envVars = ("MinVerBuildMetadata", "build.123");
            var expected = Package.WithVersion(0, 0, 0, new[] { "alpha", "0" }, 0, "build.123");

            // act
            var (sdkActual, _) = await Sdk.BuildProject(path, envVars: envVars);
            var (cliActual, _) = await MinVerCli.Run(path, envVars: envVars);

            // assert
            Assert.Equal(expected, sdkActual);
            Assert.Equal(expected.Version, cliActual);
        }
    }
}
