using System.Runtime.CompilerServices;
using Xunit;

namespace Fixtures.Xunit;

public static class Extensions
{
    private static readonly Lazy<string> ProjectRoot = new(() =>
    {
        var candidate = new DirectoryInfo(AppContext.BaseDirectory);
        while (candidate is not null && candidate.GetFiles("*.csproj").Length == 0)
        {
            candidate = candidate.Parent;
        }

        return candidate?.FullName ?? throw new InvalidOperationException("Project root not found.");
    });

    extension(Assert)
    {
        public static async Task Expected(
            string actual,
            IReadOnlyCollection<object>? discriminators = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "")
        {
            var inferredClassName = Path.GetFileNameWithoutExtension(callerFilePath);

            var discriminator = discriminators != null && discriminators.Count != 0
                ? "_" + string.Join("_", discriminators)
                : "";

            var actualPath = Path.Combine(
                ProjectRoot.Value, $"{inferredClassName}.{callerMemberName}{discriminator}.actual.txt");

            var expectedPath = Path.Combine(
                ProjectRoot.Value, $"{inferredClassName}.{callerMemberName}{discriminator}.expected.txt");

            var expected = File.Exists(expectedPath) ? await File.ReadAllTextAsync(expectedPath) : "";

            var actualNormalized = actual.ReplaceLineEndings().Trim();
            var expectedNormalized = expected.ReplaceLineEndings().Trim();

            if (actualNormalized != expectedNormalized)
            {
                await File.WriteAllTextAsync(actualPath, actualNormalized);
            }
            else if (File.Exists(actualPath))
            {
                File.Delete(actualPath);
            }

            Assert.Equal(expectedNormalized, actualNormalized);
        }
    }
}
