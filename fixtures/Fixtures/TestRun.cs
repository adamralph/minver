namespace Fixtures;

internal static class TestRun
{
    public static long Id { get; } = DateTimeOffset.UtcNow.UtcTicks;
}
