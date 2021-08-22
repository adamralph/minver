using System;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using SimpleExec;
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
            Result result = null;
            var exception = await Record.ExceptionAsync(async () => (_, result) = await Sdk.BuildProject(path, envVars: envVars));
            var actual = exception != null ? exception.Message : result.StandardOutput;

            // assert
            Assert.Contains("MinVer: [output] MinVerVersion=2.3.4-alpha-x.5+build.6+7", actual, StringComparison.Ordinal);
            Assert.Contains("MinVer: [output] MinVerBuildMetadata=build.6+7", actual, StringComparison.Ordinal);
            Assert.Contains("MinVer: [output] PackageVersion=2.3.4-alpha-x.5+build.6+7", actual, StringComparison.Ordinal);
            Assert.Contains("MinVer: [output] Version=2.3.4-alpha-x.5+build.6+7", actual, StringComparison.Ordinal);
        }
    }
}
