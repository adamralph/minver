namespace MinVer.Lib;

#pragma warning disable CA1032 // Implement standard exception constructors
public sealed class NoGitException(string message, Exception innerException) : Exception(message, innerException)
#pragma warning restore CA1032
{
}
