using Testing;
using Tests.Packages.Infra;
using Xunit;
using static SimpleExec.Command;

[assembly: AssemblyFixture(typeof(PackagesFixture))]

namespace Tests.Packages.Infra;

public sealed class PackagesFixture
{
    public PackagesFixture() =>
        Run("dotnet", $"pack --configuration {Solution.Configuration} --output artifacts", Solution.GetFullPath("."));
}
