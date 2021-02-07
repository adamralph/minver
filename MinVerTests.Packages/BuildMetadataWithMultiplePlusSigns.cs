using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class BuildMetadataWithMultiplePlusSigns
    {
        [Fact]
        public static async Task IsUsed()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);
            var envVars = ("MinVerVersionOverride".ToAltCase(), "2.3.4-alpha-x.5+build.6+7");

            // act
            // SemVer doesn't allow multiple plus signs, but MinVer doesn't care
            string @out = null;
            var exception = await Record.ExceptionAsync(async () => (_, @out) = await Sdk.BuildProject(path, envVars: envVars));
            if (exception != null)
            {
                @out = exception.Message;
            }

            // assert
            Assert.Contains("MinVer: [output] MinVerVersion=2.3.4-alpha-x.5+build.6+7", @out);
            Assert.Contains("MinVer: [output] MinVerBuildMetadata=build.6+7", @out);
            Assert.Contains("MinVer: [output] PackageVersion=2.3.4-alpha-x.5+build.6+7", @out);
            Assert.Contains("MinVer: [output] Version=2.3.4-alpha-x.5+build.6+7", @out);
        }
    }
}
