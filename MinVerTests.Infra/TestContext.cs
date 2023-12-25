using System;

namespace MinVerTests.Infra;

internal static class TestContext
{
    public static long RunId { get; } = DateTimeOffset.UtcNow.UtcTicks;
}
