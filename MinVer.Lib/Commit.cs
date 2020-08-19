namespace MinVer.Lib
{
    using System.Collections.Generic;

    internal class Commit
    {
        public Commit(string sha, bool subtreeRoot = false) => (this.Sha, this.SubtreeRoot) = (sha, subtreeRoot);

        public string Sha { get; }

        public string ShortSha => this.Sha.Substring(0, 7);

        public bool SubtreeRoot { get; } = false;

        public List<Commit> Parents { get; } = new List<Commit>();
    }
}
