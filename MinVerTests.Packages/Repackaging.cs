using System;
using System.Reflection;
using System.Threading.Tasks;
using MinVerTests.Infra;
using Xunit;

namespace MinVerTests.Packages;

public static class Repackaging
{
    [RestrictedTheory(
        new[] { "^3.*", "^5.*", },
        new[] { "OSX", },
        "With an SDK less than what's being used to run this test, or on macOS, there is sometimes a 15 minute delay after the `dotnet build` command when multi-targeting")]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task DoesNotRecreatePackage(bool multiTarget)
    {
        // arrange
        var path = MethodBase.GetCurrentMethod().GetTestDirectory(multiTarget);
        await Sdk.CreateProject(path, multiTarget: multiTarget);

        await Git.Init(path);
        await Git.Commit(path);
        await Git.Tag(path, "2.3.4");

        var (_, standardOutput, _) = await Sdk.BuildProject(path);

        Assert.Contains("Successfully created package", standardOutput, StringComparison.OrdinalIgnoreCase);

        // act
        (standardOutput, _) = await Sdk.Pack(path);

        // assert
        Assert.DoesNotContain("Successfully created package", standardOutput, StringComparison.OrdinalIgnoreCase);
    }
}
