using System.IO;
using System.Threading;

namespace MinVerTests.Infra
{
    // The spin waits are required. System.IO and the file system race. ¯\_(ツ)_/¯
    public static class FileSystem
    {
#pragma warning disable CA1802 // Use literals where appropriate
        private static readonly int millisecondsTimeout = 50;
#pragma warning restore CA1802 // Use literals where appropriate

        public static void EnsureEmptyDirectory(string path)
        {
            if (SpinWait.SpinUntil(() => Directory.Exists(path), millisecondsTimeout))
            {
                DeleteDirectory(path);
            }

            CreateDirectory(path);
        }

        private static void DeleteDirectory(string path)
        {
            // Directory.Delete fails if anything in the tree has the read-only attribute set. ¯\_(ツ)_/¯
            ResetAttributes(new DirectoryInfo(path));

#if NET
            static void ResetAttributes(DirectoryInfo directory)
#else
            void ResetAttributes(DirectoryInfo directory)
#endif
            {
                foreach (var childDirectory in directory.GetDirectories())
                {
                    ResetAttributes(childDirectory);
                }

                foreach (var file in directory.GetFiles())
                {
                    file.Attributes = FileAttributes.Normal;
                }

                directory.Attributes = FileAttributes.Normal;
            }

            Directory.Delete(path, true);

            if (!SpinWait.SpinUntil(() => !Directory.Exists(path), millisecondsTimeout))
            {
                throw new IOException($"Failed to delete directory '{path}'.");
            }
        }

        private static void CreateDirectory(string path)
        {
            _ = Directory.CreateDirectory(path);

            if (!SpinWait.SpinUntil(() => Directory.Exists(path), millisecondsTimeout))
            {
                throw new IOException($"Failed to create directory '{path}'.");
            }
        }
    }
}
