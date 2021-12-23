using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace MinVerTests.Lib.Infra
{
    internal static class AssertFile
    {
        public static async Task Contains(string expectedPath, string actual)
        {
            var actualPath = Path.Combine(
                Path.GetDirectoryName(expectedPath) ?? "",
                Path.GetFileNameWithoutExtension(expectedPath) + "-actual" + Path.GetExtension(expectedPath));

            if (File.Exists(actualPath))
            {
                File.Delete(actualPath);
            }

            var expected = await File.ReadAllTextAsync(expectedPath);

            expected = Normalize(expected);
            actual = Normalize(actual);

            try
            {
                Assert.Equal(expected, actual);
            }
            catch (EqualException ex)
            {
                await File.WriteAllTextAsync(actualPath, actual, Encoding.UTF8);

                throw new XunitException(
                    $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Expected file: {expectedPath}{Environment.NewLine}Actual file: {actualPath}");
            }
        }

        private static string Normalize(string text)
        {
            text = text
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\r", "\n", StringComparison.Ordinal)
                .Replace(" \n", "\n", StringComparison.Ordinal)
                .Replace("\n", "\r\n", StringComparison.Ordinal);

            if (!text.EndsWith("\r\n", StringComparison.Ordinal))
            {
                text += "\r\n";
            }

            return text;
        }
    }
}
