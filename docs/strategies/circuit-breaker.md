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

## Patterns and Anti-patterns
Throughout the years many people have used Polly in so many different ways. Some reoccuring patterns are suboptimal. So, this section shows the donts and dos.

### 1 - Using different sleep duration between retry attempts based on Circuit Breaker state

Imagine that we have an inner Circuit Breaker and an outer Retry strategies.
We would like to define the retry in a way that the sleep duration calculation is taking into account the Circuit Breaker's state.

❌ DON'T

Use closure to branch based on circuit breaker state

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
- By default each strategy is independent and do not have any reference to other strategies
- Here we are using the (`stateProvider`) to access the Circuit Breaker's state
  - Which is still suboptimal since the retry strategy's `DelayGenerator` branches based on state
- Also this solution is fragile since the break duration and the sleep duration are not connected
  - If a future maintainer of code changes the `circuitBreaker`'s `BreakDuration` then (s)he might forget to change sleep duration as well

✅ DO

Use `Context` to pass information between strategies

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
- With this approach the two strategies are less coupled
  - They both use the context and the `sleepDurationKey` components
- The Circuit Breaker shares the `BreakDuration` via the context whenever it breaks
  - When it transitions back to Closed then it "revokes" the sharring
- The Retry do not have any circuit breaker specific logic
  - It dynamically fetches the sleep duration whithout knowing anything about the circuit breaker
  - Also if `BreakDuration` needs to be adjusted then it can be done in a single place

### 2 - Using different duration for breaks

In case of Retry you can specify dynamically the sleep duration via the `DelayGenerator`.
In case of Circuit Breaker the `BreakDuration` is considered constant (can't be changed between breaks).

❌ DON'T

Use `Task.Delay` inside `OnOpened`

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
- The lowest value of break duration is half second
  - That means each sleep is actually takes `sleepDurationProvider.Current` + half second
- You might think that setting the `BreakDuration` to the `sleepDurationProvider.Current` solves the problem
  - But it doesn't since the `BreakDuration` is set only once and not re-evalutated at every break

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

The `CircuitBreakerStartegyOptions` currently do not provide a way to define break durations dynamically.
This might be re-evaluted later until that use the first example as a workaround with caution.

### 3 - Wrapping each endpoint with a circuit breaker

Imagine that you have to call N number of services via `HttpClient`s.
You want to decorate all downstream calls with the service-aware Circuit Breaker.

❌ DON'T

Use a collection of Circuit Breakers and explicitly call `ExecuteAsync`

<!-- snippet: circuit-breaker-anti-pattern-3 -->
```cs
// Defined in a common place
var uriToCbMappings = new Dictionary<Uri, ResiliencePipeline>
{
    { new Uri("https://downstream1.com"), GetCircuitBreaker() },
    // ...
    { new Uri("https://downstreamN.com"), GetCircuitBreaker() }
};

// Used in the downstream 1 client
var downstream1Uri = new Uri("https://downstream1.com");
await uriToCbMappings[downstream1Uri].ExecuteAsync(CallXYZOnDownstream1, CancellationToken.None);
```
<!-- endSnippet -->

**Reasoning**:
- In every places where you use an `HttpClient` you have to have a reference to the  `uriToCbMappings`
- It is your responsibility to decorate each and every network call with the related circuit breaker

✅ DO

Use named and typed `HttpClient`s

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
- The HttpClient - Circuit Breaker integrations are done at startup time
- There is no need to explicitly call `ExecuteAsync` since the underlying `DelegatingHandler` will do it on your behalf
