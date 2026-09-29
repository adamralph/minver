using Testing;
using Tests.Packages.Infra;
using Xunit;
using static SimpleExec.Command;

[assembly: AssemblyFixture(typeof(PackagesFixture))]

namespace Tests.Packages.Infra;

#pragma warning disable CA1515 // Consider making public types internal
public sealed class PackagesFixture
#pragma warning restore CA1515
{
    public PackagesFixture() =>
        Run("dotnet", $"pack --configuration {Solution.Configuration} --output artifacts", Solution.GetFullPath("."));
}
