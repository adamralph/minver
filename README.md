<img src="assets/min-ver.png" width="100" />

# MinVer

_[![nuget](https://img.shields.io/nuget/v/MinVer.svg?style=flat)](https://www.nuget.org/packages/MinVer)_
_[![Build status](https://ci.appveyor.com/api/projects/status/0ai8j3x4tg6w3ima/branch/master?svg=true)](https://ci.appveyor.com/project/adamralph/min-ver/branch/master)_

A minimalistic [.NET package](https://www.nuget.org/packages/MinVer) for versioning .NET SDK-style projects using Git tags.

- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)
- [Usage](#usage)
- [FAQ](#faq)

## Prerequisites

- [.NET Core SDK 2.1.300 or later](https://www.microsoft.com/net/download)
- [libcurl](https://curl.haxx.se/libcurl/) (Linux only)

## Quick start

- Install [MinVer](https://www.nuget.org/packages/MinVer).
- Build your project.

Your project will be versioned according to the latest tag found in the commit history.

## Usage

At a point in time, on a given branch, you work in a `MAJOR.MINOR` range, e.g. `1.0`, `1.1`, or `2.0`. The branch could be `master`, `develop`, a special release branch, a support branch, or anything else. When you want to release from that branch, whether it's a pre-release, RTM, patch, or anything else, simply create a tag with a valid [SemVer](https://semver.org) version. MinVer will apply that version to the assemblies and packages when you build your projects.

Before you create the first tag, interim builds have a default version of `0.0.0-alpha.0`. If you prefer those interim builds to have a version within the current range, set the `MinVerMajorMinor` MSBuild property. For example:

```xml
<PropertyGroup>
  <MinVerMajorMinor>1.0<MinVerMajorMinor>
</PropertyGroup>
```

MinVer will now use a default version of `1.0.0-alpha.0`.

If you begin to release versions in the `1.0` range from another branch (e.g. a special release branch), update this value to `1.1`, `2.0`, or whatever `MAJOR.MINOR` range the current branch now represents.

If the current commit is not tagged, MinVer searches its ancestors for the latest tag. If the latest tag is a [pre-release](https://semver.org/#spec-item-9), MinVer will use it as-is. If the latest tag is RTM (not pre-release), MinVer will increase the patch number and add the default pre-release identifiers, e.g. `1.0.0` becomes `1.0.1-alpha.0`.

You will notice that MinVer adds another number to the pre-release identifiers when the current commit is not tagged. This is the number of commits since the last tag, or if no tag was found, since the root commit. This is known as "height". For example, if the last tag is `1.0.0-beta.1`, at a height of 42 commits, the calculated version is `1.0.0-beta.1.42`.

## FAQ

_(With TL;DR answers inline.)_

- [Can I use my own pre-release versioning scheme?](#can-i-use-my-own-pre-release-versioning-scheme) _(yes)_
- [Can I prefix my tag names?](#can-i-prefix-my-tag-names) _(yes)_
- [Does MinVer work with my chosen branching strategy?](#does-minver-work-with-my-chosen-branching-strategy) _(yes)_
- [Can I include build metadata in the version?](#can-i-include-build-metadata-in-the-version) _(yes)_
- [Can I use the version calculated by MinVer for other purposes?](#can-i-use-the-version-calculated-by-minver-for-other-purposes) _(yes)_
- [Can I get more detailed logs?](#can-i-get-more-detailed-logs) _(yes)_
- [What takes precedence? The environment variable, or the MSBuild property?](#what-takes-precedence-the-environment-variable-or-the-msbuild-property) _(the env var)_
- [What if the history diverges, and more than one tag is found?](#what-if-the-history-diverges-and-more-than-one-tag-is-found) _(nothing bad)_
- [What if the history diverges, and then converges again, before the last tag (or root commit) is found?](#what-if-the-history-diverges-and-then-converges-again-before-the-last-tag-or-root-commit-is-found) _(nothing bad)_
- [Why does MinVer fail with `LibGit2Sharp.NotFoundException`?](#why-does-minver-fail-with-libgit2sharpnotfoundexception) _(easy to fix)_
- [Why does MinVer fail with `System.TypeInitializationException`?](#why-does-minver-fail-with-systemtypeinitializationexception) _(easy to fix)_
- [What if it all goes wrong?](#what-if-it-all-goes-wrong) _(don't panic!)_

### Can I use my own pre-release versioning scheme?

Yes! MinVer doesn't care what your pre-release versioning scheme is. The default pre-release identifiers are `alpha.0`, but you can use whatever you like in your tags. If your versioning scheme is valid [SemVer](https://semver.org), it will work with MinVer.

For example, all these versions work with MinVer:

- `1.0.0-beta.1`
- `1.0.0-pre.1`
- `1.0.0-preview-20181104`
- `1.0.0-rc.1`

### Can I prefix my tag names?

Yes! Set the prefix in the MSBuild property `MinVerTagPrefix` (or environment variable `MINVER_TAG_PREFIX`).

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

Yes! Specify [build metadata](https://semver.org/#spec-item-10) in the environment variable `MINVER_BUILD_METADATA`.

For example, in [`appveyor.yml`](https://www.appveyor.com/docs/appveyor-yml/):

```yaml
environment:
  MINVER_BUILD_METADATA: build.%APPVEYOR_BUILD_NUMBER%
```

### Can I use the version calculated by MinVer for other purposes?

Yes! MinVer sets the `MinVerVersion`, `Version`, and `PackageVersion` MSBuild properties identically. Use them in a target which runs after MinVer. E.g.

```xml
<Target Name="MyTarget" AfterTargets="MinVer">
  <Message Text="MinVerVersion=$(MinVerVersion)" Importance="high" />
  <Message Text="Version=$(Version)" Importance="high" />
  <Message Text="PackageVersion=$(PackageVersion)" Importance="high" />
</Target>
```

### What if the history diverges, and more than one tag is found?

The tag with the higher version is used.

### What if the history diverges, and then converges again, before the last tag (or root commit) is found?

The height on the first branch followed is used. The first branch followed is the one with the older parent.

### Can I get more detailed logs?

Yes! Set the MSBuild property `MinVerVerbose` (or environment variable `MINVER_VERBOSE`) to `true`. You will see how many commits were examined, which version tags were found but ignored, which version was calculated, etc.

### What takes precedence? The environment variable, or the MSBuild property?

Some values described here can be set by either an environment variable or an MSBuild property. The environment variable always takes precedence. This allows you to temporarily change those settings by altering your build server config.

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

If MinVer calculates an unexpected version and you can't figure out why, but you need to ship your software in the meantime, set a temporary version override in the environment variable `MINVER_VERSION` (or MSBuild property `MinVerVersion`) and build your project again.

**Important:** This is a _complete_ override which disables _all_ versioning logic in MinVer. It must include the full version, including any required [pre-release identifiers](https://semver.org/#spec-item-9) and [build metadata](https://semver.org/#spec-item-10).

For example, in [Appveyor](https://www.appveyor.com/):

```
1.2.3-beta.4+build.%APPVEYOR_BUILD_NUMBER%  // pre-release with build metadata
1.2.3-beta.4                                // pre-release without build metadata
1.2.3+build.%APPVEYOR_BUILD_NUMBER%         // RTM with build metadata
1.2.3                                       // RTM without build metadata
```

The same applies if you find a bug in MinVer (consider that a challenge!) and you're waiting for a fix.

---

<sub>[Tag](https://thenounproject.com/term/tag/938952) by [Ananth](https://thenounproject.com/ananthshas/) from [the Noun Project](https://thenounproject.com/).</sub>
