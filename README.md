<img src="assets/minver.png" width="100" />

# MinVer

_[![nuget](https://img.shields.io/nuget/v/MinVer.svg?style=flat)](https://www.nuget.org/packages/MinVer)_
_[![Build status](https://ci.appveyor.com/api/projects/status/0ai8j3x4tg6w3ima/branch/master?svg=true)](https://ci.appveyor.com/project/adamralph/minver/branch/master)_

A minimalistic [.NET package](https://www.nuget.org/packages/MinVer) for versioning .NET SDK-style projects using Git tags.

Also available as a [command line tool](#can-i-use-minver-to-version-software-which-is-not-built-using-a-net-sdk-style-project) for use in any Git repository.

- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)
- [Usage](#usage)
- [Options](#options)
- [FAQ](#faq)

## Prerequisites

- [.NET Core SDK 2.1.300 or later](https://www.microsoft.com/net/download)
- [libcurl](https://curl.haxx.se/libcurl/) (Linux only)

## Quick start

- Install [MinVer](https://www.nuget.org/packages/MinVer).
- Build your project.

Your project will be versioned according to the latest tag found in the commit history.

## Usage

When you want to release a version of your software, whether it's a pre-release, RTM, patch, or anything else, simply create a tag with a name which is a valid [SemVer 2.0](https://semver.org/spec/v2.0.0.html) version and build your projects. MinVer will apply the version to the assemblies and packages. (If you like to prefix your tag names, see the [FAQ](#can-i-prefix-my-tag-names).)

When the current commit is not tagged, MinVer searches the commit history for the latest tag. If the latest tag found is a [pre-release](https://semver.org/spec/v2.0.0.html#spec-item-9), MinVer will use it as-is. If the latest tag found is RTM (not pre-release), MinVer will increase the patch number and add the default pre-release identifiers, e.g. `1.0.0` becomes `1.0.1-alpha.0`. If no tag is found, the default version `0.0.0-alpha.0` is used.

You will notice that MinVer adds another number to the pre-release identifiers when the current commit is not tagged. This is the number of commits since the latest tag, or if no tag was found, since the root commit. This is known as "height". For example, if the latest tag found is `1.0.0-beta.1`, at a height of 42 commits, the calculated version is `1.0.0-beta.1.42`.

## Options

Options can be specified as either MSBuild properties or environment variables.

- [`MinVerBuildMetadata`](#can-i-include-build-metadata-in-the-version)
- [`MinVerMajorMinor`](#how-can-i-bump-the-major-or-minor-version)
- [`MinVerTagPrefix`](#can-i-prefix-my-tag-names)
- [`MinVerVerbosity`](#can-i-control-the-logging-verbosity)
- [`MinVerVersionOverride`](#what-if-it-all-goes-wrong)

Note that the option names are case-insensitive.

## FAQ

_(With TL;DR answers inline.)_

- [How can I bump the major or minor version?](#how-can-i-bump-the-major-or-minor-version)
- [Can I use my own pre-release versioning scheme?](#can-i-use-my-own-pre-release-versioning-scheme) _(yes)_
- [Can I prefix my tag names?](#can-i-prefix-my-tag-names) _(yes)_
- [Does MinVer work with my chosen branching strategy?](#does-minver-work-with-my-chosen-branching-strategy) _(yes)_
- [Can I include build metadata in the version?](#can-i-include-build-metadata-in-the-version) _(yes)_
- [Can I use the version calculated by MinVer for other purposes?](#can-i-use-the-version-calculated-by-minver-for-other-purposes) _(yes)_
- [Can I control the logging verbosity?](#can-i-control-the-logging-verbosity) _(yes)_
- [Can I use MinVer to version software which is not built using a .NET SDK style project?](#can-i-use-minver-to-version-software-which-is-not-built-using-a-net-sdk-style-project) _(yes)_
- [What if the history diverges, and more than one tag is found?](#what-if-the-history-diverges-and-more-than-one-tag-is-found) _(nothing bad)_
- [What if the history diverges, and then converges again, before the latest tag (or root commit) is found?](#what-if-the-history-diverges-and-then-converges-again-before-the-latest-tag-or-root-commit-is-found) _(nothing bad)_
- [Why does MinVer fail with `LibGit2Sharp.NotFoundException`?](#why-does-minver-fail-with-libgit2sharpnotfoundexception) _(easy to fix)_
- [Why does MinVer fail with `System.TypeInitializationException`?](#why-does-minver-fail-with-systemtypeinitializationexception) _(easy to fix)_
- [What if it all goes wrong?](#what-if-it-all-goes-wrong) _(don't panic!)_

### How can I bump the major or minor version?

You probably want to do this because, at a point in time, on a given branch, you are working on a `MAJOR.MINOR` range, e.g. `1.0`, `1.1`, or `2.0`. The branch could be `master`, `develop`, a special release branch, a support branch, or anything else.

Before you create the first tag on that branch, interim builds will use the latest tag found in the commit history, which may not match the `MAJOR.MINOR` range which the current branch represents. Or if no tag is found in the commit history, interm builds will have the default version `0.0.0-alpha.0`. If you prefer those interim builds to have a version within the current range, specify the range with [`MinVerMajorMinor`](#options). For example:

```xml
<PropertyGroup>
  <MinVerMajorMinor>1.0<MinVerMajorMinor>
</PropertyGroup>
```

MinVer will now use a default version of `1.0.0-alpha.0`.

If you begin to release versions in the `1.0` range from another branch (e.g. a special release branch), update this value to `1.1`, `2.0`, or whatever `MAJOR.MINOR` range the current branch now represents.

Note that `MinVerMajorMinor` will be redundant after you create the first tag with same `MAJOR.MINOR`. If you don't care that the versions of interm builds before that first tag will have a lower `MAJOR.MINOR`, then simply don't specify `MinVerMajorMinor`.

Also note that if the latest tag found in the commit history has a higher `MAJOR.MINOR` than `MinVerMajorMinor`, then `MinVerMajorMinor` will be ignored.

### Can I use my own pre-release versioning scheme?

Yes! MinVer doesn't care what your pre-release versioning scheme is. The default pre-release identifiers are `alpha.0`, but you can use whatever you like in your tags. If your versioning scheme is valid [SemVer 2.0](https://semver.org/spec/v2.0.0.html), it will work with MinVer.

For example, all these versions work with MinVer:

- `1.0.0-beta.1`
- `1.0.0-pre.1`
- `1.0.0-preview-20181104`
- `1.0.0-rc.1`

### Can I prefix my tag names?

Yes! Specifying the prefix with [`MinVerTagPrefix`](#options).

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

Yes! Specify [build metadata](https://semver.org/spec/v2.0.0.html#spec-item-10) with `MinVerBuildMetadata`.

For example, in [`appveyor.yml`](https://www.appveyor.com/docs/appveyor-yml/):

```yaml
environment:
  MINVERBUILDMETADATA: build.%APPVEYOR_BUILD_NUMBER%
```

You can also specify build metadata in a version tag. If the tag is on the current commit, its build metadata will be used. If the tag is on an older commit, its build metadata will be ignored. Build metadata in the environment variable will be appended to build metadata in the tag.

### Can I use the version calculated by MinVer for other purposes?

Yes! MinVer sets both the `Version` and `PackageVersion` MSBuild properties. Use them in a target which runs after MinVer. E.g.

```xml
<Target Name="MyTarget" AfterTargets="MinVer">
  <Message Text="Version=$(Version)" Importance="high" />
  <Message Text="PackageVersion=$(PackageVersion)" Importance="high" />
</Target>
```

### Can I control the logging verbosity?

Yes! Set [`MinVerVerbosity`](#options) to `quiet`, `minimal`, `normal` (default), `detailed`, or `diagnostic`. At the `quiet` and `miminal` levels, you will see only warnings and errors. At the `detailed` and `diagnostic` levels you will see how many commits were examined, which version tags were found but ignored, which version was calculated, etc.

The verbosity levels reflect those supported by MSBuild and therefore `dotnet build`, `dotnet pack`, etc. In a future version of MinVer, these verbosity levels will be inherited from MSBuild and `MinVerVerbosity` will be deprecated. Currently this is not possible due to technical restrictions related to [libgit2](https://github.com/libgit2/libgit2).

### Can I use MinVer to version software which is not built using a .NET SDK style project?

Yes! MinVer is also available as a [command line tool](https://www.nuget.org/packages/minver-cli). Run `minver --help` for usage. The calculated version is printed to standard output (stdout).

### What if the history diverges, and more than one tag is found?

The tag with the higher version is used.

### What if the history diverges, and then converges again, before the latest tag (or root commit) is found?

The height on the first branch followed is used. The first branch followed is the one with the older parent.

### Why does MinVer fail with `LibGit2Sharp.NotFoundException`?

You may see an exception of this form:

> Unhandled Exception: LibGit2Sharp.NotFoundException: object not found - no match for id (...)

This is because you are using a [shallow clone](https://www.git-scm.com/docs/git-clone#git-clone---depthltdepthgt). MinVer uses [libgit2](https://github.com/libgit2/libgit2) to interrogate the repo and [libgit2 does not support shallow clones](https://github.com/libgit2/libgit2/issues/3058). To resolve this problem, use a regular (deep) clone.

**Important:** By default, [Travis CI](https://travis-ci.org/) uses shallow clones with a depth of 50 commits. To build on Travis CI, [remove the `--depth` flag](https://docs.travis-ci.com/user/customizing-the-build#git-clone-depth).

### Why does MinVer fail with `System.TypeInitializationException`?

You may see an exception of this form:

> Unhandled Exception: System.TypeInitializationException: The type initializer for 'LibGit2Sharp.Core.NativeMethods' threw an exception. ---> System.DllNotFoundException: Unable to load shared library 'git2-8e0b172' or one of its dependencies.

This is probably because you are running on Linux, and you do not have libcurl installed. See the [prerequisites](#prerequisites).

### What if it all goes wrong?

If MinVer calculates an unexpected version and you can't figure out why, but you need to ship your software in the meantime, you can specify a temporary override with [`MinVerVersionOverride`](#options).

**Important:** This is a _complete_ override which disables _all_ versioning logic in MinVer. It must include the full version, including any required [pre-release identifiers](https://semver.org/spec/v2.0.0.html#spec-item-9) and [build metadata](https://semver.org/spec/v2.0.0.html#spec-item-10).

For example, in the [Appveyor](https://www.appveyor.com/) UI, under _Settings → Environment → Environment variables_, add an environment variable named `MINVERVERSIONOVERRIDE`:

| E.g. to release:                                               | Set the value to:                            |
|----------------------------------------------------------------|----------------------------------------------|
| ...the fourth beta of version 1.2.3, with build metadata       | `1.2.3-beta.4+build.%APPVEYOR_BUILD_NUMBER%` |
| ...the fourth beta of version 1.2.3, without build metadata    | `1.2.3-beta.4`                               |
| ...the stable(/final/RTM) version 1.2.3 with build metadata    | `1.2.3+build.%APPVEYOR_BUILD_NUMBER%`        |
| ...the stable(/final/RTM) version 1.2.3 without build metadata | `1.2.3`                                      |

The same applies if you find a bug in MinVer (consider that a challenge!) and you're waiting for a fix.

---

<sub>[Tag](https://thenounproject.com/term/tag/938952) by [Ananth](https://thenounproject.com/ananthshas/) from [the Noun Project](https://thenounproject.com/).</sub>
