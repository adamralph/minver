using System;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class EmptyRepo
    {
        [Fact]
        public static async Task HasDefaultVersion()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);
            await Git.Init(path);
            var expected = Package.WithVersion(0, 0, 0, new[] { "alpha", "0" });

            // act
            var (actual, sdkStandardOutput, _) = await Sdk.BuildProject(path);
            var (cliStandardOutput, cliStandardError) = await MinVerCli.ReadAsync(path);

            // assert
            Assert.Equal(expected, actual);
            Assert.Contains("No commits found", sdkStandardOutput, StringComparison.Ordinal);

            Assert.Equal(expected.Version, cliStandardOutput.Trim());
            Assert.Contains("No commits found", cliStandardError, StringComparison.Ordinal);
        }
    }
}
