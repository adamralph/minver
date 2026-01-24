using MinVerTests.Infra;
using MinVerTests.Packages.Infra;
using Xunit;
using static SimpleExec.Command;

[assembly: AssemblyFixture(typeof(PackagesFixture))]

namespace MinVerTests.Packages.Infra;

public sealed class PackagesFixture
{
    public PackagesFixture() =>
        Run("dotnet", $"pack --configuration {Solution.Configuration} --output artifacts", Solution.GetFullPath("."));
}
