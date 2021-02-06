using System;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class NoRepo
    {
        [Fact]
        public static async Task HasDefaultVersion()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);
            var expected = Package.WithVersion(0, 0, 0, new[] { "alpha", "0" });

            // act
            var (sdkActual, sdkOut) = await Sdk.BuildProject(path);
            var (cliActual, cliErr) = await MinVerCli.Run(path);

            // assert
            Assert.Equal(expected, sdkActual);
            Assert.Contains("MINVER1001", sdkOut, StringComparison.Ordinal);

            Assert.Equal(expected.Version, cliActual);
            Assert.Contains("not a valid Git working directory", cliErr, StringComparison.Ordinal);
        }
    }
}
