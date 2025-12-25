namespace MinVer.Lib;

internal sealed class Commit(string sha)
{
    public string Sha { get; } = sha;

    public string ShortSha => Sha[..Math.Min(7, Sha.Length)];

    public List<Commit> Parents { get; } = [];

    public override string ToString() => ShortSha;
}
