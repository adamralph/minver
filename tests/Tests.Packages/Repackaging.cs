using System.Reflection;
using Fixtures;
using Tests.Packages.Fixtures;
using Xunit;

namespace Tests.Packages;

public static class Repackaging
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task DoesNotRecreatePackage(bool multiTarget)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory(multiTarget);
        await DotNetCli.CreateProject(path, multiTarget: multiTarget);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");

        var (_, standardOutput, _) = await DotNetCli.BuildProject(path);

        Assert.Contains("Successfully created package", standardOutput, StringComparison.OrdinalIgnoreCase);

        // act
        (standardOutput, _) = await DotNetCli.Pack(path);

        // assert
        Assert.DoesNotContain("Successfully created package", standardOutput, StringComparison.OrdinalIgnoreCase);
    }
}
