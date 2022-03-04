using System;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class Repackaging
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static async Task DoesNotRecreatePackage(bool multiTarget)
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory(tag: multiTarget);
            await Sdk.CreateProject(path, multiTarget: multiTarget);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.Tag(path, "2.3.4");

            var (_, result) = await Sdk.BuildProject(path);

            Assert.Contains("Successfully created package", result.StandardOutput, StringComparison.OrdinalIgnoreCase);

            // act
            result = await Sdk.Pack(path);

            // assert
            Assert.DoesNotContain("Successfully created package", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        }
    }
}
