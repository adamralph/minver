using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class VersionOverride
    {
        [Fact]
        public static async Task HasVersionOverride()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.Tag(path, "2.3.4");

            var envVars = ("MinVerVersionOverride".ToAltCase(), "3.4.5-alpha.6+build.7");

            var expected = Package.WithVersion(3, 4, 5, new[] { "alpha", "6" }, 0, "build.7");

            // act
            var (sdkActual, _) = await Sdk.BuildProject(path, envVars: envVars);
            var (cliActual, _) = await MinVerCli.Run(path, envVars: envVars);

            // assert
            Assert.Equal(expected, sdkActual);
            Assert.Equal(expected.Version, cliActual);
        }
    }
}
