using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages
{
    public static class GitLogPaths
    {
        [Fact]
        public static async Task AreUsed()
        {
            // arrange
            var path = MethodBase.GetCurrentMethod().GetTestDirectory();
            await Sdk.CreateProject(path);

            await Git.Init(path);
            await Git.Commit(path);

            using var _ = File.Create(Path.Combine(path, "a"));
            using var __ = File.Create(Path.Combine(path, "b"));

            await Task.Delay(1000);

            await Git.Add(path);
            await Git.Commit(path);

            var envVars = ("MinVerGitLogPaths", "a b");

            var expected = Package.WithVersion(0, 0, 0, new[] { "alpha", "0" });

            // act
            var (actual, _) = await Sdk.BuildProject(path, envVars: envVars);
            var cli = await MinVerCli.ReadAsync(path, envVars: envVars);

            // assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected.Version, cli.StandardOutput.Trim());
        }
    }
}
