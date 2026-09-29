---
applyTo: "src/Polly/**/*.cs,src/Polly.Core/**/*.cs,src/Polly.Extensions/**/*.cs,src/Polly.RateLimiting/**/*.cs,src/Polly.Testing/**/*.cs"
---

# Library Code

Conventions for the code of the Polly NuGet packages. Where an existing file's style differs from these rules, the file's style takes precedence.

## Strategy Implementation

- **Return outcomes, don't throw.** Strategies communicate failures by returning an `Outcome<T>` (e.g. `Outcome.FromException<TResult>(...)`) from `ExecuteCore`, rather than throwing. Exceptions thrown by user callbacks must be captured into an outcome, not allowed to escape the pipeline unexpectedly.
- **Honor `ContinueOnCapturedContext`.** Awaits in strategies and pipeline infrastructure use `.ConfigureAwait(context.ContinueOnCapturedContext)` so that the caller's choice is respected. A plain `.ConfigureAwait(false)`, or no `ConfigureAwait`, on a path that runs user code is a bug.
- **Support synchronous execution.** The same strategy code serves both the sync and async `Execute` APIs. Check `ResilienceContext.IsSynchronous` handling, and use the existing helpers (e.g. `TimeProviderExtensions.DelayAsync`) rather than introducing blocking or sync-over-async code.
- **Use `TimeProvider` for all time.** Never read the system clock or delay directly (`DateTime.UtcNow`, `DateTimeOffset.UtcNow`, `Stopwatch`, `Task.Delay` without a `TimeProvider`). Use the `TimeProvider` supplied through the strategy's `StrategyBuilderContext` so that behavior is deterministic and testable. Some, but not all, of these APIs are banned by `eng/analyzers/BannedSymbols.txt`.
- **Cancellation.** Respect `ResilienceContext.CancellationToken`. Linked or created `CancellationTokenSource` instances must be disposed (or returned to a pool), and any token restored on the context after execution must be the caller's original token.
- **Telemetry.** Strategies report events through `ResilienceStrategyTelemetry`. New events, or changes to existing events, are observable behavior changes for users of `Polly.Extensions` telemetry. They must be called out and documented in `docs/advanced/telemetry.md`.
- **Options validation.** Options are validated with data annotations (e.g. `[Range]`, `[Required]`) when the pipeline is built. New options should be validated in the same way, with their valid range and default value documented.
- **Thread-safety.** A built pipeline is shared and executed concurrently. Strategies must not store per-execution state in fields; per-execution state belongs in locals or on the `ResilienceContext`. Stateful components (e.g. the circuit breaker's `CircuitStateController`) must be safe under concurrent executions.
- **Disposal and reloads.** Pipelines created through `Polly.Extensions` can be reloaded and disposed. Components that own resources must release them when the pipeline is disposed, and must not be used after disposal.
- **Sibling code.** A fix or pattern in one strategy often applies to others (retry, circuit breaker, fallback, hedging, timeout, rate limiter and the chaos strategies in `src/Polly.Core/Simmy`), and to both the generic (`Foo<TResult>`) and non-generic variants of a type.
