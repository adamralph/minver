using System;
using System.Collections.Generic;

namespace MinVer.Lib;

internal class Commit
{
    public Commit(string sha) => this.Sha = sha;

    public string Sha { get; }

    public string ShortSha => this.Sha[..Math.Min(7, this.Sha.Length)];

    public List<Commit> Parents { get; } = new();

    public override string ToString() => this.ShortSha;
}
