using System;

namespace MinVer.Lib;

#pragma warning disable CA1032 // Implement standard exception constructors
public class NoGitException : Exception
#pragma warning restore CA1032
{
    public NoGitException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
