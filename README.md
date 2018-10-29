<img src="assets/min-ver.png" width="100" />

# MinVer

_[![nuget](https://img.shields.io/nuget/v/MinVer.svg?style=flat)](https://www.nuget.org/packages/MinVer)_
_[![Build status](https://ci.appveyor.com/api/projects/status/0ai8j3x4tg6w3ima/branch/master?svg=true)](https://ci.appveyor.com/project/adamralph/min-ver/branch/master)_

A minimalistic [.NET package](https://www.nuget.org/packages/MinVer) for versioning .NET projects using Git tags.

## Prerequisites

- [.NET Core SDK](https://www.microsoft.com/net/download)
- [libcurl](https://curl.haxx.se/libcurl/) (Linux only)

## Quick start

1. `dotnet add package MinVer`
2. `dotnet build`

Your project will be versioned according to the latest tag found in the commit history.

## How it works

### Inputs

- The last tag on HEAD or it's ancestors which represents a [SemVer](https://semver.org) version number._\*_
- By how many commits HEAD is ahead of the tag (known as "height")._\*\*_

\* _Each time the history diverges, the last tag is found on each path and the tag with the latest version number is used._

\*\* _Each time the history converges, the height on the first path followed is used._

### Algorithm

- If the height is zero (i.e. the tag is on HEAD), then the HEAD version matches the tag.
- If the height is non-zero (i.e. the tag is on an older commit), then:
  - If the last tag is an RTM version, `MAJOR.MINOR.PATCH`, then the HEAD version is `MAJOR.MINOR.PATCH+1-alpha.0.{height}`.
  - If the last tag is a pre-release version, `MAJOR.MINOR.PATCH-{pre-release identifiers}`, then the HEAD version is `MAJOR.MINOR.PATCH-{pre-release identifiers}.{height}`.

## FAQ

### Can I prefix my tag names?

Yes! You can specify a prefix in an environment variable or MSBuild property named `MINVER_TAG_PREFIX` or `MinVerTagPrefix`.

For example, if you prefix your tag names with "v", e.g. `v1.2.3`:

```xml
<PropertyGroup>
  <MinVerTagPrefix>v</MinVerTagPrefix>
</PropertyGroup>
```

### Does MinVer work with my chosen branching strategy?

Yes! MinVer doesn't care about branches. It's all about the tags!

That means MinVer is compatible with [Git Flow](https://nvie.com/posts/a-successful-git-branching-model/), [GitHub Flow](https://guides.github.com/introduction/flow/), [Release Flow](https://docs.microsoft.com/en-us/azure/devops/learn/devops-at-microsoft/release-flow), and any other exotic flow.

### Can I include build metadata in the version?

Yes, you can specify [build metadata](https://semver.org/#spec-item-10) in an environment variable or MSBuild property named `MINVER_BUILD_METADATA` or `MinVerBuildMetadata`.

### Can I use the version calculated by MinVer for other purposes?

Yes, but you need to move your usage of the `Version` or `PackageVersion` properties to a target which runs after MinVer. E.g.

```xml
<Target Name="MyTarget" AfterTargets="MinVer">
  <PropertyGroup>
    <Foo>$(Version)</Foo>
    <Bar>$(PackageVersion)</Bar>
  </PropertyGroup>
</Target>
```

### What if it all goes wrong?

If your tags get into a mess and you can't find a way out, you can specify a version override in an environment variable or MSBuild property named `MINVER_VERSION` or `MinVerVersion` until you've figured out the problem.

The same applies if you find a bug in MinVer (consider that a challenge!) and you're waiting for a fix, but you need to ship your software in the meantime.

### Why does MinVer fail with `LibGit2Sharp.NotFoundException`?

You may see an exception of this form:

> Unhandled Exception: LibGit2Sharp.NotFoundException: object not found - no match for id (...)

This is because you are using a [shallow clone](https://www.git-scm.com/docs/git-clone#git-clone---depthltdepthgt). MinVer uses [libgit2](https://github.com/libgit2/libgit2) to interrogate the repo and [libgit2 does not support shallow clones](https://github.com/libgit2/libgit2/issues/3058). To resolve this problem, use a regular (deep) clone.

Note that, by default, [Travis CI](https://travis-ci.org/) uses shallow clones with a depth of 50 commits. To build on Travis CI, [remove the `--depth` flag](https://docs.travis-ci.com/user/customizing-the-build#git-clone-depth).

### Why does MinVer fail with `System.TypeInitializationException`?

You may see an exception of this form:

> Unhandled Exception: System.TypeInitializationException: The type initializer for 'LibGit2Sharp.Core.NativeMethods' threw an exception. ---> System.DllNotFoundException: Unable to load shared library 'git2-8e0b172' or one of its dependencies.

This is probably because you are running on Linux, and you do not have libcurl installed. See the [prerequisites](#prerequisites).

---

<sub>[Tag](https://thenounproject.com/term/tag/938952) by [Ananth](https://thenounproject.com/ananthshas/) from [the Noun Project](https://thenounproject.com/).</sub>
