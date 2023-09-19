# Circuit breaker resilience strategy

## About

- **Options**:
  - [`CircuitBreakerStrategyOptions`](xref:Polly.CircuitBreaker.CircuitBreakerStrategyOptions)
  - [`CircuitBreakerStrategyOptions<T>`](xref:Polly.CircuitBreaker.CircuitBreakerStrategyOptions)
- **Extensions**: `AddCircuitBreaker`
- **Strategy Type**: Reactive
- **Exceptions**:
  - `BrokenCircuitException`: Thrown when a circuit is broken and the action could not be executed.
  - `IsolatedCircuitException`: Thrown when a circuit is isolated (held open) by manual override.

---

> [!NOTE]
> Version 8 documentation for this strategy has not yet been migrated. For more information on circuit breaker concepts and behavior, refer to the [older documentation](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker).

> [!NOTE]
> Be aware that the Circuit Breaker strategy [rethrows all exceptions](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#exception-handling), including those that are handled. A Circuit Breaker's role is to monitor faults and break the circuit when a certain threshold is reached; it does not manage retries. Combine the Circuit Breaker with a Retry strategy if needed.

## Usage

<!-- snippet: circuit-breaker -->
```cs
// Add circuit breaker with default options.
// See https://www.pollydocs.org/strategies/circuit-breaker#defaults for defaults.
new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions());

// Add circuit breaker with customized options:
//
// The circuit will break if more than 50% of actions result in handled exceptions,
// within any 10-second sampling duration, and at least 8 actions are processed.
new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,
    SamplingDuration = TimeSpan.FromSeconds(10),
    MinimumThroughput = 8,
    BreakDuration = TimeSpan.FromSeconds(30),
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>()
});

// Handle specific failed results for HttpResponseMessage:
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<SomeExceptionType>()
            .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
    });

// Monitor the circuit state, useful for health reporting:
var stateProvider = new CircuitBreakerStateProvider();

new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddCircuitBreaker(new() { StateProvider = stateProvider })
    .Build();

/*
CircuitState.Closed - Normal operation; actions are executed.
CircuitState.Open - Circuit is open; actions are blocked.
CircuitState.HalfOpen - Recovery state after break duration expires; actions are permitted.
CircuitState.Isolated - Circuit is manually held open; actions are blocked.
*/

// Manually control the Circuit Breaker state:
var manualControl = new CircuitBreakerManualControl();

new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new() { ManualControl = manualControl })
    .Build();

// Manually isolate a circuit, e.g., to isolate a downstream service.
await manualControl.IsolateAsync();

// Manually close the circuit to allow actions to be executed again.
await manualControl.CloseAsync();
```
<!-- endSnippet -->

## Defaults

| Property            | Default Value                                                              | Description                                                                                |
| ------------------- | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| `ShouldHandle`      | Predicate that handles all exceptions except `OperationCanceledException`. | Specifies which results and exceptions are managed by the circuit breaker strategy.        |
| `FailureRatio`      | 0.1                                                                        | The ratio of failures to successes that will cause the circuit to break/open.              |
| `MinimumThroughput` | 100                                                                        | The minimum number of actions that must occur in the circuit within a specific time slice. |
| `SamplingDuration`  | 30 seconds                                                                 | The time period over which failure ratios are calculated.                                  |
| `BreakDuration`     | 5 seconds                                                                  | The time period for which the circuit will remain broken/open before attempting to reset.  |
| `OnClosed`          | `null`                                                                     | Event triggered when the circuit transitions to the `Closed` state.                        |
| `OnOpened`          | `null`                                                                     | Event triggered when the circuit transitions to the `Opened` state.                        |
| `OnHalfOpened`      | `null`                                                                     | Event triggered when the circuit transitions to the `HalfOpened` state.                    |
| `ManualControl`     | `null`                                                                     | Allows for manual control to isolate or close the circuit.                                 |
| `StateProvider`     | `null`                                                                     | Enables the retrieval of the current state of the circuit.                                 |

## Resources

- [Making the Netflix API More Resilient](https://techblog.netflix.com/2011/12/making-netflix-api-more-resilient.html)
- [Circuit Breaker by Martin Fowler](https://martinfowler.com/bliki/CircuitBreaker.html)
- [Circuit Breaker Pattern by Microsoft](https://msdn.microsoft.com/en-us/library/dn589784.aspx)
- [Original Circuit Breaking Article](https://web.archive.org/web/20160106203951/http://thatextramile.be/blog/2008/05/the-circuit-breaker)
