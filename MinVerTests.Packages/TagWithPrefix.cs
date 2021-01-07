using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class TagWithPrefix
    {
        [Fact]
        public static async Task HasTagVersion()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.Tag(path, "v.2.3.4-alpha.5");

            var envVars = ("MinVerTagPrefix", "v.");

            var expected = Package.WithVersion(2, 3, 4, new[] { "alpha", "5" });

            // act
            var (sdkActual, _) = await Sdk.BuildProject(path, envVars: envVars);
            var (cliActual, _) = await MinVerCli.Run(path, envVars: envVars);

            // assert
            Assert.Equal(expected, sdkActual);
            Assert.Equal(expected.Version, cliActual);
        }
    }
}
