# Coding Agent Instructions

This file provides guidance to agents when working with code in this repository.

## Build Commands

```bash
# Full build (clean, restore, build, test, package)
./build.ps1

# Build only (no tests)
dotnet build

# Run all tests
dotnet test

# Run a specific test project
dotnet test ./test/Polly.Core.Tests/Polly.Core.Tests.csproj

# Run a single test by filter
dotnet test ./test/Polly.Core.Tests --filter "FullyQualifiedName~CircuitBreakerTests"

# Run tests for a specific framework
dotnet test ./test/Polly.Core.Tests --framework net10.0

# Mutation testing
./build.ps1 -Target MutationTestsCore
./build.ps1 -Target MutationTestsExtensions
./build.ps1 -Target MutationTestsLegacy
./build.ps1 -Target MutationTestsRateLimiting
./build.ps1 -Target MutationTestsTestingSupport
```

Lint runs in CI via GitHub Actions workflows (actionlint, zizmor, PSScriptAnalyzer).

Linting for C# is enabled through analyzers configured to run during the build process.

## Architecture

Polly is a .NET resilience library. Version 8+ (`Polly.Core`) is a complete redesign; `Polly` (the root package) is the legacy pre-v8 API kept for backwards compatibility.

### Core abstractions (`src/Polly.Core`)

- **`ResiliencePipeline`** / **`ResiliencePipeline<T>`** — the main user-facing type. Wraps one or more strategies and executes them in sequence.
- **`ResiliencePipelineBuilder`** / **`ResiliencePipelineBuilder<T>`** — fluent builder used to compose strategies.
- **`ResilienceStrategy`** / **`ResilienceStrategy<T>`** — base class for all built-in and custom strategies.
- **`ResilienceContext`** — per-execution context flowing through the pipeline (cancellation token, properties, result type, etc.).

### Built-in strategies

Strategies split into two categories:

**Reactive** (respond to failures):

- `RetryResilienceStrategy` — configurable backoff, jitter, attempt limits
- `CircuitBreakerResilienceStrategy` — state machine: Closed → Open → HalfOpen
- `FallbackResilienceStrategy` — returns an alternative value/action on failure
- `HedgingResilienceStrategy` — fires parallel attempts and returns the fastest success

**Proactive** (prevent overload):

- `TimeoutResilienceStrategy` — bounds execution duration
- `RateLimiterResilienceStrategy` (in `Polly.RateLimiting`) — wraps `System.Threading.RateLimiting`

Each strategy has a corresponding `*Options` class (e.g., `RetryStrategyOptions`) that holds configuration.
Predicates for which outcomes to handle are declared via the `PredicateBuilder` fluent API.

### Supporting packages

- **`Polly.Extensions`** — `IServiceCollection` extensions and telemetry/OpenTelemetry support.
- **`Polly.Testing`** — `ResiliencePipelineDescriptor` and test helpers; accesses internals via `InternalsVisibleTo`.

### Multi-targeting

- `Polly.Core`: targets .NET Framework, .NET and .NET Standard.
- Test projects: Target all supported versions of .NET plus the latest version of .NET Framework on Windows.
- Targets for .NET 8 and later support native AoT.

### Build system

- **Cake** (bootstrapped by `build.ps1`) orchestrates the full pipeline via `cake.cs`.
- **Centralized package management**: all NuGet versions in `Directory.Packages.props`.
- **`Directory.Build.props` / `eng/Common.props`**: shared MSBuild properties (nullable enabled, warnings-as-errors, strong naming via `Polly.snk`, XML doc generation).
- **`LegacySupport`**: a shared source project injected into `Polly` at compile time via a custom MSBuild target.
- **MinVer** provides automatic SemVer versioning from git tags.

### Testing patterns

- **xUnit** for unit tests; **FsCheck** for property-based/fuzz tests.
- Test projects access internals via `InternalsVisibleTo` declared in `Directory.Build.props`.
- `Polly.TestUtils` is a shared utilities project referenced by all test projects.
- `Polly.AotTest` validates AOT compatibility by publishing a trimmed app.
- Coverage is collected in CI and uploaded to Codecov.

## General guidelines

- Always ensure code compiles with no warnings or errors and tests pass locally before pushing changes.
- Do not change the public API unless specifically requested.
- Do not use APIs marked with `[Obsolete]`.
- Bug fixes should **always** include a test that would fail without the corresponding fix.
- Do not introduce new dependencies unless specifically requested.
- Do not update existing dependencies unless specifically requested.
