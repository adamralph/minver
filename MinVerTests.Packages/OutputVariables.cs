using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class OutputVariables
    {
        [Fact]
        public static async Task AreSet()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);
            var envVars = ("MinVerVersionOverride".ToAltCase(), "2.3.4-alpha-x.5+build.6");

            // act
            var (_, @out) = await Sdk.BuildProject(path, envVars: envVars);

            // assert
            Assert.Contains("MinVer: [output] MinVerVersion=2.3.4-alpha-x.5+build.6", @out);
            Assert.Contains("MinVer: [output] MinVerMajor=2", @out);
            Assert.Contains("MinVer: [output] MinVerMinor=3", @out);
            Assert.Contains("MinVer: [output] MinVerPatch=4", @out);
            Assert.Contains("MinVer: [output] MinVerPreRelease=alpha-x.5", @out);
            Assert.Contains("MinVer: [output] MinVerBuildMetadata=build.6", @out);
            Assert.Contains("MinVer: [output] AssemblyVersion=2.0.0.0", @out);
            Assert.Contains("MinVer: [output] FileVersion=2.3.4.0", @out);
            Assert.Contains("MinVer: [output] PackageVersion=2.3.4-alpha-x.5+build.6", @out);
            Assert.Contains("MinVer: [output] Version=2.3.4-alpha-x.5+build.6", @out);
        }
    }
}
