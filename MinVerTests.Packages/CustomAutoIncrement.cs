using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class CustomAutoIncrement
    {
        [Fact]
        public static async Task HasCustomAutoIncrement()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.Tag(path, "2.3.4");
            await Git.Commit(path);

            var envVars = ("MinVerAutoIncrement".ToAltCase(), "minor");

            var expected = Package.WithVersion(2, 4, 0, new[] { "alpha", "0" }, 1);

            // act
            var (sdkActual, _) = await Sdk.BuildProject(path, envVars: envVars);
            var (cliActual, _) = await MinVerCli.Run(path, envVars: envVars);

            // assert
            Assert.Equal(expected, sdkActual);
            Assert.Equal(expected.Version, cliActual);
        }
    }
}
