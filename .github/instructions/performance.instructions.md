---
applyTo: "src/Polly/**/*.cs,src/Polly.Core/**/*.cs,src/Polly.Extensions/**/*.cs,src/Polly.RateLimiting/**/*.cs,src/Polly.Testing/**/*.cs"
---

# Performance and Allocations

Polly's execution paths are allocation-free in the common case (see `docs/advanced/performance.md`), and users rely on this.

- Flag new allocations on the execution path: closures capturing variables, boxing of value types, LINQ, `params` arrays, string formatting or interpolation for events that may not be enabled, and `async` state machines where a synchronously-completed `ValueTask` would suffice.
- Prefer the existing patterns: static lambdas with explicit state arguments, `ResilienceContextPool` for contexts (every `Get()` needs a matching `Return()`, including on exception paths), pooled objects, and `readonly struct` arguments types.
- Changes to hot paths, and PRs that claim a performance improvement, should include BenchmarkDotNet results (see the `performance-benchmark` skill in `.github/skills`).
