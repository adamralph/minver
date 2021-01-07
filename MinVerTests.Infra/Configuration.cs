namespace MinVerTests.Infra
{
    internal static class Configuration
    {
        public const string Current =
#if DEBUG
            "Debug";
#elif RELEASE
            "Release";
#endif
    }
}
