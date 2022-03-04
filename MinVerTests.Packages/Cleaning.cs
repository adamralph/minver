using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class Cleaning
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static async Task PackagesAreCleaned(bool multiTarget)
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory(tag: multiTarget);
            await Sdk.CreateProject(path, multiTarget: multiTarget);

            await Git.Init(path);
            await Git.Commit(path);
            await Git.Tag(path, "2.3.4");

            _ = await Sdk.BuildProject(path);

            var packages = new Matcher().AddInclude("**/bin/Debug/*.nupkg");
            Assert.NotEmpty(packages.GetResultsInFullPath(path));

            // act
            _ = await Sdk.DotNet("clean", path, new Dictionary<string, string> { { "GeneratePackageOnBuild", "true" } });

            // assert
            Assert.Empty(packages.GetResultsInFullPath(path));
        }
    }
}
