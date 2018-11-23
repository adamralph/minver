namespace MinVer.Lib
{
    using LibGit2Sharp;

    public static class RepositoryEx
    {
        public static bool TryCreateRepo(string path, out Repository repository)
        {
            repository = default;

            path = Repository.Discover(path);

            if (path == default)
            {
                return false;
            }

            repository = new Repository(path);

            return true;
        }
    }
}
