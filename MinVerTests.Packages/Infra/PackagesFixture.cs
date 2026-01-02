using MinVerTests.Infra;
using MinVerTests.Packages.Infra;
using Xunit;
using static SimpleExec.Command;

[assembly: AssemblyFixture(typeof(PackagesFixture))]

namespace MinVerTests.Packages.Infra;

public sealed class PackagesFixture
{
    public PackagesFixture()
    {
        var solutionFolder = Solution.GetFullPath(".");
        var artifactsFolder = Solution.GetFullPath("artifacts");

        if (Directory.Exists(artifactsFolder) && Directory.EnumerateFiles(artifactsFolder, "*.nupkg").Count() == 2)
        {
            return;
        }

        Run(
            "dotnet",
            $"pack --configuration {Solution.Configuration} --output artifacts",
            solutionFolder);
    }
}
