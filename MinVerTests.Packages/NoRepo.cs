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
            var (actual, sdk) = await Sdk.BuildProject(path);
            var cli = await MinVerCli.ReadAsync(path);

            // assert
            Assert.Equal(expected, actual);
            Assert.Contains("MINVER1001", sdk.StandardOutput, StringComparison.Ordinal);

            Assert.Equal(expected.Version, cli.StandardOutput.Trim());
            Assert.Contains("not a valid Git working directory", cli.StandardError, StringComparison.Ordinal);
        }
    }
}
