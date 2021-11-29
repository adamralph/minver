using System;
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
            string actual;

            // act
            try
            {
                actual = (await Sdk.BuildProject(path, envVars: envVars)).Result.StandardOutput;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // SemVer doesn't allow multiple plus signs, but MinVer doesn't care
                actual = ex.Message;
            }

            // assert
            Assert.Contains("MinVer: [output] MinVerVersion=2.3.4-alpha-x.5+build.6+7", actual, StringComparison.Ordinal);
            Assert.Contains("MinVer: [output] MinVerBuildMetadata=build.6+7", actual, StringComparison.Ordinal);
            Assert.Contains("MinVer: [output] PackageVersion=2.3.4-alpha-x.5+build.6+7", actual, StringComparison.Ordinal);
            Assert.Contains("MinVer: [output] Version=2.3.4-alpha-x.5+build.6+7", actual, StringComparison.Ordinal);
        }
    }
}
