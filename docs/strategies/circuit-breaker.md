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

## Patterns and anti-patterns

Over the years, many developers have used Polly in various ways. Some of these recurring patterns may not be ideal. This section highlights the recommended practices and those to avoid.

### 1 - Using different sleep duration between retry attempts based on Circuit Breaker state

Imagine that we have an inner Circuit Breaker and an outer Retry strategies.

We would like to define the retry in a way that the sleep duration calculation is taking into account the Circuit Breaker's state.

❌ DON'T

Use a closure to branch based on circuit breaker state:

<!-- snippet: circuit-breaker-anti-pattern-1 -->
```cs
var stateProvider = new CircuitBreakerStateProvider();
var circuitBreaker = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new()
    {
        ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
        BreakDuration = TimeSpan.FromSeconds(5),
        StateProvider = stateProvider
    })
    .Build();

var retry = new ResiliencePipelineBuilder()
    .AddRetry(new()
    {
        ShouldHandle = new PredicateBuilder()
            .Handle<HttpRequestException>()
            .Handle<BrokenCircuitException>(),
        DelayGenerator = args =>
        {
            TimeSpan? delay = TimeSpan.FromSeconds(1);
            if (stateProvider.CircuitState == CircuitState.Open)
            {
                delay = TimeSpan.FromSeconds(5);
            }

            return ValueTask.FromResult(delay);
        }
    })
    .Build();
```
<!-- endSnippet -->

**Reasoning**:

- By default, each strategy is independent and has no any reference to other strategies.
- We use the (`stateProvider`) to access the Circuit Breaker's state. However, this approach is not optimal as the retry strategy's `DelayGenerator` varies based on state.
- This solution is delicate because the break duration and the sleep duration aren't linked.
- If a future code maintainer modifies the `circuitBreaker`'s `BreakDuration`, they might overlook adjusting the sleep duration.

✅ DO

Use `Context` to pass information between strategies:

<!-- snippet: circuit-breaker-pattern-1 -->
```cs
var circuitBreaker = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new()
    {
        ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
        BreakDuration = TimeSpan.FromSeconds(5),
        OnOpened = static args =>
        {
            args.Context.Properties.Set(SleepDurationKey, args.BreakDuration);
            return ValueTask.CompletedTask;
        },
        OnClosed = args =>
        {
            args.Context.Properties.Set(SleepDurationKey, null);
            return ValueTask.CompletedTask;
        }
    })
    .Build();

var retry = new ResiliencePipelineBuilder()
    .AddRetry(new()
    {
        ShouldHandle = new PredicateBuilder()
            .Handle<HttpRequestException>()
            .Handle<BrokenCircuitException>(),
        DelayGenerator = static args =>
        {
            _ = args.Context.Properties.TryGetValue(SleepDurationKey, out var delay);
            delay ??= TimeSpan.FromSeconds(1);
            return ValueTask.FromResult(delay);
        }
    })
    .Build();
```
<!-- endSnippet -->

**Reasoning**:

- Both strategies are less coupled in this approach since they rely on the context and the `sleepDurationKey` components.
- The Circuit Breaker shares the `BreakDuration` through the context when it breaks. When it transitions back to Closed, the sharring is revoked.
- The Retry strategy fetches the sleep duration dynamically without knowing any specific knowledge about the Circuit Breaker.
- If adjustments are needed for the `BreakDuration`, they can be made in one place.

### 2 - Using different duration for breaks

In the case of Retry you can specify dynamically the sleep duration via the `DelayGenerator`.

In the case of Circuit Breaker the `BreakDuration` is considered constant (can't be changed between breaks).

❌ DON'T

Use `Task.Delay` inside `OnOpened`:

<!-- snippet: circuit-breaker-anti-pattern-2 -->
```cs
static IEnumerable<TimeSpan> GetSleepDuration()
{
    for (int i = 1; i < 10; i++)
    {
        yield return TimeSpan.FromSeconds(i);
    }
}

var sleepDurationProvider = GetSleepDuration().GetEnumerator();
sleepDurationProvider.MoveNext();

var circuitBreaker = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new()
    {
        ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
        BreakDuration = TimeSpan.FromSeconds(0.5),
        OnOpened = async args =>
        {
            await Task.Delay(sleepDurationProvider.Current);
            sleepDurationProvider.MoveNext();
        }

    })
    .Build();
```
<!-- endSnippet -->

**Reasoning**:

- The minimum break duration value is half a second. This implies that each sleep lasts for `sleepDurationProvider.Current` plus an additional half a second.
- One might think that setting the `BreakDuration` to `sleepDurationProvider.Current` would address this, but it doesn't. This is because the `BreakDuration` is established only once and isn't re-assessed during each break.

<!-- snippet: circuit-breaker-anti-pattern-2-ext -->
```cs
circuitBreaker = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new()
    {
        ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
        BreakDuration = sleepDurationProvider.Current,
        OnOpened = async args =>
        {
            Console.WriteLine($"Break: {sleepDurationProvider.Current}");
            sleepDurationProvider.MoveNext();
        }

    })
    .Build();
```
<!-- endSnippet -->

✅ DO

The `CircuitBreakerStartegyOptions` currently do not support defining break durations dynamically. This may be re-evaluted in the future. For now, refer to the first example for a potential workaround. However, please use it with caution.

### 3 - Wrapping each endpoint with a circuit breaker

Imagine that you have to call N number of services via `HttpClient`s.
You want to decorate all downstream calls with the service-aware Circuit Breaker.

❌ DON'T

Use a collection of Circuit Breakers and explicitly call `ExecuteAsync()`:

<!-- snippet: circuit-breaker-anti-pattern-3 -->
```cs
// Defined in a common place
var uriToCbMappings = new Dictionary<Uri, ResiliencePipeline>
{
    [new Uri("https://downstream1.com")] = GetCircuitBreaker(),
    // ...
    [new Uri("https://downstreamN.com")] = GetCircuitBreaker()
};

// Used in the downstream 1 client
var downstream1Uri = new Uri("https://downstream1.com");
await uriToCbMappings[downstream1Uri].ExecuteAsync(CallXYZOnDownstream1, CancellationToken.None);
```
<!-- endSnippet -->

**Reasoning**:

- Whenever you use an `HttpClient`, you must have a reference to the `uriToCbMappings` dictionary.
- It's your responsibility to decorate each network call with the corresponding circuit breaker.

✅ DO

Use named and typed `HttpClient`s:

```cs
foreach (string uri in uris)
{
    builder.Services
      .AddHttpClient<IResilientClient, ResilientClient>(uri, client => client.BaseAddress = new Uri(uri))
      .AddPolicyHandler(GetCircuitBreaker().AsAsyncPolicy<HttpResponseMessage>());
}

...
private const string serviceUrl = "https://downstream1.com";
public Downstream1Client(
   IHttpClientFactory namedClientFactory,
   ITypedHttpClientFactory<ResilientClient> typedClientFactory)
{
    var namedClient = namedClientFactory.CreateClient(serviceUrl);
    var namedTypedClient = typedClientFactory.CreateClient(namedClient);
    ...
}
```

**Reasoning**:

- The `HttpClient` integrates with Circuit Breaker during startup.
- There's no need to call `ExecuteAsync()` directly. The `DelegatingHandler` handles it automatically.

> [!NOTE]
> The above sample code used the `AsAsyncPolicy<HttpResponseMessage>()` method to convert the `ResiliencePipeline<HttpResponseMessage>` to `IAsyncPolicy<HttpResponseMessage>`.
> It is required because the `AddPolicyHandler()` method anticipates an `IAsyncPolicy<HttpResponse>` parameter.
> Please be aware that, later an `AddResilienceHandler()` will be introduced in the `Microsoft.Extensions.Http.Resilience` package which is the successor of the `Microsoft.Extensions.Http.Polly`.
