using System.Text;
using Xunit;
using Xunit.Sdk;

namespace MinVerTests.Lib.Infra;

internal static class AssertFile
{
    public static async Task Contains(string expectedPath, string actual)
    {
        var actualPath = Path.Combine(
            Path.GetDirectoryName(expectedPath) ?? "",
            Path.GetFileNameWithoutExtension(expectedPath) + "-actual" + Path.GetExtension(expectedPath));

        var expected = await File.ReadAllTextAsync(expectedPath);

        expected = Normalize(expected);
        actual = Normalize(actual);

        try
        {
            Assert.Equal(expected, actual);
        }
        catch (EqualException ex)
        {
            if (File.Exists(actualPath))
            {
                File.Delete(actualPath);
            }

            await File.WriteAllTextAsync(actualPath, actual, Encoding.UTF8);

            throw new XunitException(
                $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Expected file: {expectedPath}{Environment.NewLine}Actual file: {actualPath}");
        }
    }

    internal static readonly string[] separator = ["\r\n"];

    private static string Normalize(string text)
    {
        text = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace(" \n", "\n", StringComparison.Ordinal)
            .Replace("\n", "\r\n", StringComparison.Ordinal);

        text = string.Join("\r\n", text.Split(separator, StringSplitOptions.None).Select(line => line.TrimEnd()));

        if (!text.EndsWith("\r\n", StringComparison.Ordinal))
        {
            text += "\r\n";
        }

        return text;
    }
}
