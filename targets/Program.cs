using System.Threading.Tasks;
using static Bullseye.Targets;
using static SimpleExec.Command;

internal class Program
{
    public static Task Main(string[] args)
    {
        Target("default", DependsOn("test", "pack"));

        Target("build", () => RunAsync("dotnet", "build MinVer.sln --configuration Release"));

        Target(
            "test",
            DependsOn("build"),
            () => RunAsync("dotnet", $"test ./MinVerTests/MinVerTests.csproj --configuration Release --no-build --verbosity=normal"));

        Target(
            "publish",
            DependsOn("build"),
            () => RunAsync("dotnet", $"publish ./MinVer.Cli/MinVer.Cli.csproj --configuration Release --no-build"));

        Target(
            "pack",
            DependsOn("publish"),
            () => RunAsync("dotnet", $"pack ./MinVer.Cli/MinVer.Cli.csproj --configuration Release --no-build"));

        return RunTargetsAsync(args);
    }
}
