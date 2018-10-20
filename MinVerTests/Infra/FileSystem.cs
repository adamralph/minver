namespace MinVerTests.Infra
{
    using System.IO;
    using System.Threading;

    // The spin waits are required. System.IO and the file system race. ¯\_(ツ)_/¯
    public static class FileSystem
    {
        private static readonly int millisecondsTimeout = 50;

        public static void EnsureDirectoryCreated(string path)
        {
            if (SpinWait.SpinUntil(() => !Directory.Exists(path), millisecondsTimeout))
            {
                CreateDirectory(path);
            }
        }

        public static void EnsureDirectoryDeleted(string path)
        {
            if (SpinWait.SpinUntil(() => Directory.Exists(path), millisecondsTimeout))
            {
                DeleteDirectory(path);
            }
        }

        private static void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);

            if (!SpinWait.SpinUntil(() => Directory.Exists(path), millisecondsTimeout))
            {
                throw new IOException($"Failed to create directory '{path}'.");
            }
        }

        private static void DeleteDirectory(string path)
        {
            // Directory.Delete fails if anything in the tree has the read-only attibute set. ¯\_(ツ)_/¯
            ResetAttributes(new DirectoryInfo(path));

            void ResetAttributes(DirectoryInfo directory)
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
    }
}
