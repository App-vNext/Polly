---
applyTo: "test/**"
---

# Tests

- Test projects use xUnit, Shouldly for assertions, NSubstitute for mocking, `FakeTimeProvider` for time, and FsCheck for property-based tests. Shared helpers live in `test/Polly.TestUtils`. Use these rather than introducing new libraries.
- **Bug fixes must include a regression test** that would fail without the fix. Tests for specific GitHub issues typically live in `test/Polly.Core.Tests/Issues`, named after the issue number (e.g. `IssuesTests.InfiniteRetry_2163.cs`). Check that the test actually exercises the reported scenario.
- Tests must be deterministic. Flag tests that depend on real delays, wall-clock time, `Thread.Sleep`, or timing-sensitive races. Advance a `FakeTimeProvider` instead.
- When a change affects them, cover both the generic and non-generic, and sync and async, execution paths.
- CI enforces code coverage and runs mutation tests. Tests should assert on meaningful behavior (outcomes, event arguments, invocation counts) so that mutants are killed, rather than just executing code for coverage.
- Test code may use internal types through `InternalsVisibleTo`. Do not make types or members `public` in `src/` just for testing.
