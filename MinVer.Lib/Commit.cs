using System;
using System.Collections.Generic;

namespace MinVer.Lib;

internal sealed class Commit(string sha)
{
    public string Sha { get; } = sha;

    public string ShortSha => this.Sha[..Math.Min(7, this.Sha.Length)];

    public List<Commit> Parents { get; } = [];

    public override string ToString() => this.ShortSha;
}
