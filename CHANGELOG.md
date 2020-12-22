# Changelog

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
- [#28: Default version when the directory is not a git repo](https://github.com/adamralph/minver/issues/28)
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
