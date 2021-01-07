using System;
using System.IO;
using System.Threading.Tasks;

namespace MinVerTests.Lib.Infra
{
    public static class AssertFile
    {
        public static async Task Contains(string expectedPath, string actual)
        {
            if (actual != await File.ReadAllTextAsync(expectedPath))
            {
                var actualPath = Path.Combine(
                    Path.GetDirectoryName(expectedPath),
                    Path.GetFileNameWithoutExtension(expectedPath) + "-actual" + Path.GetExtension(expectedPath));

                await File.WriteAllTextAsync(actualPath, actual);

                throw new Exception($"{actualPath} does not contain the contents of {expectedPath}.");
            }
        }
    }
}
