using System;
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
        [RestrictedTheory(
            new[] { "^3.*", "^5.*" },
            new[] { "OSX" },
            "With an SDK less than what's being used to run this test, or on macOS, there is sometimes a 15 minute delay after the `dotnet build` command when multi-targeting")]
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

        [Fact]
        public static async Task MinVerDoesNotRunWhenPackagesAreNotGeneratedOnBuild()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            // act
            var (standardOutput, _) = await Sdk.DotNet(
                "clean",
                path,
                new Dictionary<string, string>
                {
                    { "GeneratePackageOnBuild", "false" },
                    { "MinVerVerbosity", "diagnostic" },
                });

            // assert
            Assert.DoesNotContain("minver:", standardOutput, StringComparison.OrdinalIgnoreCase);
        }
    }
}
