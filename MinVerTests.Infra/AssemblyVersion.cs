#if !NETCOREAPP2_1
namespace MinVerTests.Infra
{
    public record AssemblyVersion(int Major, int Minor, int Build, int Revision);
}
#endif
