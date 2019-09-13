namespace MinVer.Lib
{
    using System.IO;
    using LibGit2Sharp;

    internal static class RepositoryEx
    {
        public static bool TryCreateRepo(string repoOrWorkDir, out Repository repository)
        {
            repository = null;

            while (repoOrWorkDir != null)
            {
                try
                {
                    repository = new Repository(repoOrWorkDir);
                    return true;
                }
                catch (RepositoryNotFoundException)
                {
                    repoOrWorkDir = Directory.GetParent(repoOrWorkDir)?.FullName;
                }
            }

            return false;
        }
    }
}
