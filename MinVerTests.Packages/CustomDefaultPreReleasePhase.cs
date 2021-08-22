using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class CustomDefaultPreReleasePhase
    {
        [Fact]
        public static async Task HasCustomDefaultPreReleasePhase()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.Tag(path, "2.3.4");
            await Git.Commit(path);

            var envVars = ("MinVerDefaultPreReleasePhase".ToAltCase(), "preview");

            var expected = Package.WithVersion(2, 3, 5, new[] { "preview", "0" }, 1);

            // act
            var (actual, _) = await Sdk.BuildProject(path, envVars: envVars);
            var cli = await MinVerCli.ReadAsync(path, envVars: envVars);

            // assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected.Version, cli.StandardOutput.Trim());
        }
    }
}
