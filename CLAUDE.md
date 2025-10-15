# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WoofWare.Expect is an F# expect/snapshot testing library (similar to Jest snapshots). The project consists of two main components:

- **WoofWare.Expect**: Core library that provides the `expect` computation expression and snapshot comparison functionality
- **WoofWare.Expect.Test**: Test suite using NUnit that demonstrates and validates the library functionality

## Build and Development Commands

This project uses Nix for development environment management. All commands should be run within the Nix development shell:

```bash
# Restore dependencies
nix develop --command dotnet restore

# Build the project
nix develop --command dotnet build --configuration Release

# Run tests
nix develop --command dotnet test

# Pack NuGet package
nix develop --command dotnet pack --configuration Release
```

### Code Formatting and Analysis

```bash
# Format F# code (via Nix)
nix run .#fantomas -- .

# Check formatting without modifying files
nix run .#fantomas -- --check .

# Run F# analyzers
nix run .#fsharp-analyzers -- --project ./WoofWare.Expect/WoofWare.Expect.fsproj --analyzers-path ./.analyzerpackages/g-research.fsharp.analyzers/*/
```

### Single Test Execution

NUnit's filtering is pretty borked.
You can't apply filters that contain special characters in the test name (like a space character).
You have to do e.g. `FullyQualifiedName~singleword` rather than `FullyQualifiedName~single word test`, but this only works on tests whose names are single words to begin with.

Instead of running `dotnet test`, you can perform a build (`dotnet build`) and then run `dotnet woofware.nunittestrunner WoofWare.Expect.Test/bin/Debug/net9.0/WoofWare.Expect.Test.dll`.
This is an NUnit test runner which accepts a `--filter` arg that takes the same filter syntax as `dotnet test`, but actually parses it correctly: test names can contain spaces.
(The most foolproof way to provide test names to WoofWare.NUnitTestRunner is by XML-encoding: e.g. `FullyQualifiedName="MyNamespace.MyTestsClass&lt;ParameterType1%2CParameterType2&gt;.MyTestMethod"`. The `~` query operator is also supported.)

## Architecture

### Core Components

- **Builder.fs**: Contains the `ExpectBuilder` computation expression that powers the `expect` syntax
- **Domain.fs**: Core types including `CallerInfo`, `ExpectState`, and `SnapshotValue`
- **SnapshotUpdate.fs**: Handles updating snapshot files when tests fail
- **AstWalker.fs**: Uses Fantomas.FCS to parse F# source and locate/update snapshot strings
- **Diff.fs**: Provides Patience and Myers diff algorithms for readable test output
- **Dot.fs**: ASCII art rendering of dot files (requires `graph-easy`)

### Key Design Patterns

- **Computation Expression**: The library is built around F#'s computation expression syntax with `expect { }` blocks
- **Source Code Manipulation**: Uses F# compiler services to locate and update snapshot strings in source files
- **Dual Mode Operation**: Tests can run in assertion mode (normal) or update mode (to fix failing snapshots)

### Snapshot Types

- `snapshot`: Plain text comparison using `ToString()`
- `snapshotJson`: JSON serialization with configurable options
- `snapshotList`: Formatted list comparison
- `snapshotThrows`: Exception expectation testing
- `withFormat`: Custom formatting functions
- `withJsonSerializerOptions`: Custom JSON serialization
- `withJsonDocOptions`: Custom JSON parsing (e.g., for comments)

### Test Snapshot Management

- Individual updates: Add `'` to `expect` (making it `expect'`), run test to update, then remove `'`
- Bulk updates: Use `GlobalBuilderConfig.enterBulkUpdateMode()` in test setup and `GlobalBuilderConfig.updateAllSnapshots()` in teardown

## Important Notes

- Use verbatim string literals for snapshots - the update mechanism requires this
- Avoid format strings (`$"..."`) or string concatenation in snapshot definitions
- The project follows strict F# conventions with warnings as errors
- All commits go through comprehensive CI including formatting, analysis, and testing
