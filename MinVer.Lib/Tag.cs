namespace MinVer.Lib
{
    internal class Tag
    {
        public Tag(string name, string sha)
        {
            this.Name = name;
            this.Sha = sha;
        }

        public string Name { get; }

        public string Sha { get; }
    }
}
