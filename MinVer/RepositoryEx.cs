namespace MinVer
{
    using System.IO;
    using LibGit2Sharp;

    public static class RepositoryEx
    {
        public static bool TryCreateRepo(string path, out Repository repository)
        {
            repository = default;

            while (path != default)
            {
                try
                {
                    repository = new Repository(path);
                    return true;
                }
                catch (RepositoryNotFoundException)
                {
                    path = Directory.GetParent(path)?.FullName;
                }
            }

            return false;
        }
    }
}
