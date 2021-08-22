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
            var (actual, sdk) = await Sdk.BuildProject(path);
            var cli = await MinVerCli.ReadAsync(path);

            // assert
            Assert.Equal(expected, actual);
            Assert.Contains("No commits found", sdk.StandardOutput, StringComparison.Ordinal);

            Assert.Equal(expected.Version, cli.StandardOutput.Trim());
            Assert.Contains("No commits found", cli.StandardError, StringComparison.Ordinal);
        }
    }
}
