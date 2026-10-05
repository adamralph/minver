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

    public static async Task Verify(
        this string received,
        string suffix = "",
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "")
    {
        var inferredClassName = Path.GetFileNameWithoutExtension(callerFilePath);

        var receivedPath = Path.Combine(
            ProjectRoot.Value, $"{inferredClassName}.{callerMemberName}{suffix}.received.txt");

        var verifiedPath = Path.Combine(
            ProjectRoot.Value, $"{inferredClassName}.{callerMemberName}{suffix}.verified.txt");

        var verified = File.Exists(verifiedPath) ? await File.ReadAllTextAsync(verifiedPath) : "";

        var receivedNormalized = received.ReplaceLineEndings().Trim();
        var verifiedNormalized = verified.ReplaceLineEndings().Trim();

        if (receivedNormalized != verifiedNormalized)
        {
            await File.WriteAllTextAsync(receivedPath, receivedNormalized);
        }
        else if (File.Exists(receivedPath))
        {
            File.Delete(receivedPath);
        }

        Assert.Equal(verifiedNormalized, receivedNormalized);
    }
}
