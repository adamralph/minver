# Changelog

## 5.0.0

### Enhancements

- [#861: Bump NuGet.Versioning from 6.4.0 to 6.5.0](https://github.com/adamralph/minver/pull/861)
- [#879: Bump NuGet.Versioning from 6.5.0 to 6.6.0](https://github.com/adamralph/minver/pull/879)
- [#901: Bump NuGet.Versioning from 6.6.0 to 6.7.0](https://github.com/adamralph/minver/pull/901)
- [#909: Bump McMaster.Extensions.CommandLineUtils from 4.0.2 to 4.1.0](https://github.com/adamralph/minver/pull/909)

### Other

- [#844: **[BREAKING]** drop support for .NET Core](https://github.com/adamralph/minver/pull/844)

## 4.3.0

### Enhancements

- [#794: Bump NuGet.Versioning from 6.2.1 to 6.3.0](https://github.com/adamralph/minver/pull/794)
- [#818: Bump NuGet.Versioning from 6.3.0 to 6.3.1](https://github.com/adamralph/minver/pull/818)
- [#836: Bump NuGet.Versioning from 6.3.1 to 6.4.0](https://github.com/adamralph/minver/pull/836)
- [#839: replace default pre-release phase with default pre-release identifiers](https://github.com/adamralph/minver/pull/839)
- [#841: Bump McMaster.Extensions.CommandLineUtils from 4.0.1 to 4.0.2](https://github.com/adamralph/minver/pull/841)

## 4.2.0

### Enhancements

- [#774: Bump NuGet.Versioning from 6.2.0 to 6.2.1](https://github.com/adamralph/minver/pull/774)
- [#779: Produce an MSBuild error when "git" is not present in PATH](https://github.com/adamralph/minver/issues/779)

## 4.1.0

### Enhancements

- [#767: .NET 6 binaries](https://github.com/adamralph/minver/issues/767)

## 4.0.0

### Enhancements

- [#720: **[BREAKING]** Restrict versions to SemVer 2.0 only](https://github.com/adamralph/minver/pull/720)

## 3.1.0

### Enhancements

- [#653: Bump McMaster.Extensions.CommandLineUtils from 3.1.0 to 4.0.0](https://github.com/adamralph/minver/pull/653)
- [#697: Option to ignore height](https://github.com/adamralph/minver/issues/697)

## 3.0.0

### Enhancements

- [#549: add README.md to packages](https://github.com/adamralph/minver/pull/549)

### Fixed bugs

- [#542: source stepping doesn't work](https://github.com/adamralph/minver/pull/542)
- [#589: **[BREAKING]** An empty working directory argument or option (deprecated) is ignored in minver-cli](https://github.com/adamralph/minver/pull/589)
- [#665: Cannot set auto-increment to patch in the command-line tool when the env var is set to something else](https://github.com/adamralph/minver/issues/665)
- [#666: Cannot set verbosity to info in the command-line tool when the env var is set to something else](https://github.com/adamralph/minver/issues/666)
- [#682: Packages are redundantly recreated](https://github.com/adamralph/minver/issues/682)
- [#683: Packages are not cleaned](https://github.com/adamralph/minver/issues/683)

### Other

- [#451: **[BREAKING]** Remove deprecated working directory option from minver-cli](https://github.com/adamralph/minver/issues/451)
- [#586: **[BREAKING]** drop support for .NET Core 2.1](https://github.com/adamralph/minver/pull/586)

## 2.5.0

### Enhancements

- [#477: Bump McMaster.Extensions.CommandLineUtils from 3.0.0 to 3.1.0](https://github.com/adamralph/minver/pull/477)

### Fixed bugs

- [#479: MinVerPreRelease terminates before the first hyphen in the pre-release identifiers](https://github.com/adamralph/minver/issues/479)
- [#480: MinVerBuildMetadata terminates before the first plus sign in the build metadata identifiers](https://github.com/adamralph/minver/issues/480)

## 2.4.0

### Enhancements

- [#395: clarify that MINVER1001 refers to a Git working directory](https://github.com/adamralph/minver/pull/395)
- [#436: Accept working directory as an optional argument in minver-cli](https://github.com/adamralph/minver/issues/436)
- [#447: align error message for invalid verbosity value](https://github.com/adamralph/minver/pull/447)
- [#450: Use environment variables in minver-cli](https://github.com/adamralph/minver/issues/450)

## 2.3.1

### Fixed bugs

- [#377: first root commit is not used](https://github.com/adamralph/minver/pull/377)
- [#378: first tag with a given version is not used](https://github.com/adamralph/minver/pull/378)

## 2.3.0

### Enhancements

- [#347: Support .NET Core SDK 3.1.300 and later](https://github.com/adamralph/minver/pull/347)

## 2.2.0

### Enhancements

- [#312: upgrade to SourceLink 1.0.0](https://github.com/adamralph/minver/pull/312)
- [#313: upgrade McMaster.Extensions.CommandLineUtils from 2.3.4 to 2.4.4](https://github.com/adamralph/minver/pull/313)
- [#319: Simpler, more accurate exception message when git is not present in PATH](https://github.com/adamralph/minver/pull/319)

## 2.1.0

### Enhancements

- [#303: Allow disabling MinVer](https://github.com/adamralph/minver/pull/303)

## 2.0.0

### Enhancements

- [#244: run on machines with only later versions of .NET Core than 2.x](https://github.com/adamralph/minver/issues/244)
- [#269: Support shallow clones](https://github.com/adamralph/minver/issues/269)

### Fixed bugs

- [#149: Segmentation Fault in Alpine 3.8](https://github.com/adamralph/minver/issues/149)

### Other

- [#256: **[BREAKING]** switch from LibGit2Sharp to Git](https://github.com/adamralph/minver/pull/256)

## 1.2.0

### Enhancements

- [#246: Configurable default pre-release phase](https://github.com/adamralph/minver/issues/246)
- [#253: Update SourceLink to 1.0.0-beta2-19367-01](https://github.com/adamralph/minver/issues/253)

## 1.1.0

### Enhancements

- [#209: Suppress NU5105 in consuming projects](https://github.com/adamralph/minver/issues/209)
- [#212: Option to increment minor or major version instead of patch](https://github.com/adamralph/minver/issues/212)

### Fixed bugs

- [#217: MinVer can fail if internal item groups are already populated](https://github.com/adamralph/minver/issues/217)

## 1.0.0

### Enhancements

- [#1: Determine version for a commit](https://github.com/adamralph/minver/issues/1)
- [#2: Default version when there no commits](https://github.com/adamralph/minver/issues/2)
- [#3: MSBuild integration](https://github.com/adamralph/minver/issues/3)
- [#28: Default version when the directory is not a Git repository](https://github.com/adamralph/minver/issues/28)
- [#49: Tag prefixes](https://github.com/adamralph/minver/pull/49)
- [#57: add verbose logging option](https://github.com/adamralph/minver/pull/57)
- [#63: Specify the minimum major and minor version](https://github.com/adamralph/minver/issues/63)
- [#94: tool package](https://github.com/adamralph/minver/issues/94)
- [#109: Output the version number parts as properties](https://github.com/adamralph/minver/issues/109)
- [#116: Set assembly version and file version](https://github.com/adamralph/minver/issues/116)
- [#147: Output all properties at detailed level](https://github.com/adamralph/minver/issues/147)
- [#151: debug/detailed level message when MinVerMajorMinor is redundant](https://github.com/adamralph/minver/issues/151)
- [#154: MinVerPreRelease and MinVerBuildMetadata output variables](https://github.com/adamralph/minver/issues/154)
- [#170: Hide version from console when MSBuild log level is quiet/minimal/normal](https://github.com/adamralph/minver/issues/170)
- [#180: Add MinVerVersionOverride property to simplify usage of both MinVer and minver-cli in the same build](https://github.com/adamralph/minver/issues/180)
- [#193: Log info message when no version tags are found](https://github.com/adamralph/minver/issues/193)
- [#194: Log all ignored tags for a commit](https://github.com/adamralph/minver/issues/194)
