using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;
using Xunit.Abstractions;

namespace MinVerTests.Packages
{
    public class MultipleProjects
    {
        private readonly ITestOutputHelper output;

        public MultipleProjects(ITestOutputHelper output) => this.output = output;

        [Fact]
        public async Task MultipleTagPrefixes()
        {
            void log(string message) => this.output.Log(message);

            // arrange
            log("arrange");
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();

            await Sdk.CreateSolution(path, new[] { "project0", "project1", "project2", "project3" }, log: log);

            var props =
$@"<Project>

{"  "}<PropertyGroup>
{"    "}<MinVerTagPrefix>v</MinVerTagPrefix>
{"  "}</PropertyGroup>

</Project>
";

            File.WriteAllText(Path.Combine(path, "project1", "Directory.Build.props"), props);
            File.WriteAllText(Path.Combine(path, "project3", "Directory.Build.props"), props);

            await Git.Init(path, log);
            await Git.Commit(path, log);
            await Git.Tag(path, "2.3.4", log);
            await Git.Tag(path, "v5.6.7", log);

            var expected0 = Package.WithVersion(2, 3, 4);
            var expected1 = Package.WithVersion(5, 6, 7);
            var expected2 = Package.WithVersion(2, 3, 4);
            var expected3 = Package.WithVersion(5, 6, 7);

            // act
            log("act");
            var (packages, @out) = await Sdk.Build(path, log);

            // assert
            log("assert");
            Assert.NotNull(@out);

            var versionCalculations = @out
                .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.StartsWith("MinVer: Calculated version ", StringComparison.OrdinalIgnoreCase));

            Assert.Collection(
                versionCalculations,
                message => Assert.Equal("MinVer: Calculated version 2.3.4.", message),
                message => Assert.Equal("MinVer: Calculated version 5.6.7.", message),
                message => Assert.Equal("MinVer: Calculated version 2.3.4.", message),
                message => Assert.Equal("MinVer: Calculated version 5.6.7.", message));

            Assert.Collection(
                packages,
                package => Assert.Equal(expected0, package),
                package => Assert.Equal(expected1, package),
                package => Assert.Equal(expected2, package),
                package => Assert.Equal(expected3, package));
        }
    }
}
