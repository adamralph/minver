# MinVer Development Guide

## Build Commands
- Build: `./build` (Linux/macOS) or `build.cmd` (Windows)
- Format check: `dotnet format --verify-no-changes`
- Pack: `dotnet run --project targets -- pack`

## Test Commands
- Run all tests: `./build` or `build.cmd`
- Library tests: `dotnet test ./MinVerTests.Lib --framework net8.0 --configuration Release`
- Package tests: `dotnet test ./MinVerTests.Packages --configuration Release`
- Single test: `dotnet test ./MinVerTests.Lib --filter "FullyQualifiedName=MinVerTests.Lib.TestClassName.TestMethodName"`

## Code Style
- Uses .NET SDK-style projects with nullable reference types enabled
- Follows C# naming conventions (PascalCase for public members, camelCase for parameters)
- Uses implicit usings and latest C# language features
- Enforces code style in build with static analysis enabled
- Tests use xUnit with `[Fact]` attributes
- Targets .NET 8.0 and .NET 9.0