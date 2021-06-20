using System;
using Xunit.Abstractions;

namespace MinVerTests.Packages
{
    internal static class TestOutputHelperExtensions
    {
        public static void Log(this ITestOutputHelper output, string message) => output.WriteLine($"{DateTimeOffset.UtcNow:R}: {message}");
    }
}
