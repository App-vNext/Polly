# Rate limiter resilience strategy

## About

- **Options**: [`RateLimiterStrategyOptions`](xref:Polly.RateLimiting.RateLimiterStrategyOptions)
- **Extensions**: `AddRateLimiter`, `AddConcurrencyLimiter`
- **Strategy Type**: Proactive
- **Exceptions**:
  - `RateLimiterRejectedException`: Thrown when a rate limiter rejects an execution.
- **Package**: [Polly.RateLimiting](https://www.nuget.org/packages/Polly.RateLimiting)

---

The rate limiter resilience strategy controls the number of operations that can pass through it. This strategy is a thin layer over the API provided by the [`System.Threading.RateLimiting`](https://www.nuget.org/packages/System.Threading.RateLimiting) package.

Further reading:

- [Announcing rate limiting for .NET](https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/)
- [Rate limiting API documentation](https://learn.microsoft.com/dotnet/api/system.threading.ratelimiting)

## Usage

<!-- snippet: rate-limiter -->
```cs
// Add rate limiter with default options.
// See https://www.pollydocs.org/strategies/rate-limiter#defaults for defaults.
new ResiliencePipelineBuilder()
    .AddRateLimiter(new RateLimiterStrategyOptions());

// Create a rate limiter to allow a maximum of 100 concurrent executions and a queue of 50.
new ResiliencePipelineBuilder()
    .AddConcurrencyLimiter(100, 50);

// Create a rate limiter that allows 100 executions per minute.
new ResiliencePipelineBuilder()
    .AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
    {
        PermitLimit = 100,
        Window = TimeSpan.FromMinutes(1)
    }));
```
<!-- endSnippet -->

Example execution:

<!-- snippet: rate-limiter-execution -->
```cs
var pipeline = new ResiliencePipelineBuilder().AddConcurrencyLimiter(100, 50).Build();

try
{
    // Execute an asynchronous text search operation.
    var result = await pipeline.ExecuteAsync(
        token => TextSearchAsync(query, token),
        cancellationToken);
}
catch (RateLimiterRejectedException ex)
{
    // Handle RateLimiterRejectedException,
    // that can optionally contain information about when to retry.
    if (ex.RetryAfter is TimeSpan retryAfter)
    {
        Console.WriteLine($"Retry After: {retryAfter}");
    }
}
```
<!-- endSnippet -->

## Defaults

| Property                    | Default Value                                        | Description                                                                                     |
| --------------------------- | ---------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| `RateLimiter`               | `null`                                               | Generator that creates a `RateLimitLease` for executions.                                       |
| `DefaultRateLimiterOptions` | `PermitLimit` set to 1000 and `QueueLimit` set to 0. | The options for the default concurrency limiter that will be used when `RateLimiter` is `null`. |
| `OnRejected`                | `null`                                               | Event that is raised when the execution is rejected by the rate limiter.                        |

## Diagrams

### Rate Limiter

Let's suppose we have a rate limiter strategy with `PermitLimit` : `1` and `Window` : `10 seconds`.

### Rate Limiter: happy path sequence diagram

```mermaid
sequenceDiagram
    autonumber
    actor C as Caller
    participant P as Pipeline
    participant RL as RateLimiter
    participant D as DecoratedUserCallback

    C->>P: Calls ExecuteAsync
    P->>RL: Calls ExecuteCore
    Note over RL,D: Window start
    RL->>+D: Invokes
    D->>-RL: Returns result
    RL->>P: Returns result
    P->>C: Returns result
    Note over C: Several seconds later...
    Note over RL,D: Window end
    C->>P: Calls ExecuteAsync
    P->>RL: Calls ExecuteCore
    Note over RL,D: Window start
    RL->>+D: Invokes
    D->>-RL: Returns result
    RL->>P: Returns result
    P->>C: Returns result
    Note over RL,D: Window end
```

#### Rate limiter: unhappy path sequence diagram

```mermaid
sequenceDiagram
    autonumber
    actor C as Caller
    participant P as Pipeline
    participant RL as RateLimiter
    participant D as DecoratedUserCallback

    C->>P: Calls ExecuteAsync
    P->>RL: Calls ExecuteCore
    Note over RL,D: Window start
    RL->>+D: Invokes
    D->>-RL: Returns result
    RL->>P: Returns result
    P->>C: Returns result
    Note over C: Few seconds later...
    C->>P: Calls ExecuteAsync
    P->>RL: Calls ExecuteCore
    RL-->>RL: Rejects request
    RL->>P: Throws <br/>RateLimiterRejectedException
    P->>C: Propagates exception
    Note over RL,D: Window end
```

### Concurrency Limiter

Let's suppose we have a concurrency limiter strategy with `PermitLimit` : `1` and `QueueLimit` : `1`.

#### Concurrency limiter: happy path sequence diagram

```mermaid
sequenceDiagram
    actor C1 as Caller1
    actor C2 as Caller2
    participant P as Pipeline
    participant CL as ConcurrencyLimiter
    participant D as DecoratedUserCallback

    par
    C1->>P: Calls ExecuteAsync
    and
    C2->>P: Calls ExecuteAsync
    end

    P->>CL: Calls ExecuteCore
    CL->>+D: Invokes (C1)
    P->>CL: Calls ExecuteCore
    CL-->>CL: Queues request

    D->>-CL: Returns result (C1)
    CL->>P: Returns result (C1)
    CL->>+D: Invokes (C2)
    P->>C1: Returns result
    D->>-CL: Returns result (C2)
    CL->>P: Returns result (C2)
    P->>C2: Returns result
```

#### Concurrency Limiter: unhappy path sequence diagram

```mermaid
sequenceDiagram
    actor C1 as Caller1
    actor C2 as Caller2
    actor C3 as Caller3
    participant P as Pipeline
    participant CL as ConcurrencyLimiter
    participant D as DecoratedUserCallback

    par
    C1->>P: Calls ExecuteAsync
    and
    C2->>P: Calls ExecuteAsync
    and
    C3->>P: Calls ExecuteAsync
    end

    P->>CL: Calls ExecuteCore
    CL->>+D: Invokes (C1)
    P->>CL: Calls ExecuteCore
    CL-->>CL: Queues request (C2)
    P->>CL: Calls ExecuteCore
    CL-->>CL: Rejects request (C3)
    CL->>P: Throws <br/>RateLimiterRejectedException
    P->>C3: Propagates exception

    D->>-CL: Returns result (C1)
    CL->>P: Returns result (C1)
    CL->>+D: Invokes (C2)
    P->>C1: Returns result
    D->>-CL: Returns result (C2)
    CL->>P: Returns result (C2)
    P->>C2: Returns result
```

## Disposal of rate limiters

The `RateLimiter` is a disposable resource. When you explicitly create a `RateLimiter` instance, it's good practice to dispose of it once it's no longer needed. This is usually not an issue when manually creating resilience pipelines using the `ResiliencePipelineBuilder`. However, when dynamic reloads are enabled, failing to dispose of discarded rate limiters can lead to excessive resource consumption. Fortunately, Polly provides a way to dispose of discarded rate limiters, as demonstrated in the example below:

<!-- snippet: rate-limiter-disposal -->
```cs
services
    .AddResiliencePipeline("my-pipeline", (builder, context) =>
    {
        var options = context.GetOptions<ConcurrencyLimiterOptions>("my-concurrency-options");

        // This call enables dynamic reloading of the pipeline
        // when the named ConcurrencyLimiterOptions change.
        context.EnableReloads<ConcurrencyLimiterOptions>("my-concurrency-options");

        var limiter = new ConcurrencyLimiter(options);

        builder.AddRateLimiter(limiter);

        // Dispose of the limiter when the pipeline is disposed.
        context.OnPipelineDisposed(() => limiter.Dispose());
    });
```
<!-- endSnippet -->

The above example uses the `AddResiliencePipeline(...)` extension method to configure the pipeline. However, a similar approach can be taken when directly using the `ResiliencePipelineRegistry<T>`.

## Partitioned rate limiter

For advanced use-cases, the partitioned rate limiter is also available. The following example illustrates how to retrieve a partition key from `ResilienceContext` using the `GetPartitionKey` method:

<!-- snippet: rate-limiter-partitioned -->
```cs
var partitionedLimiter = PartitionedRateLimiter.Create<ResilienceContext, string>(context =>
{
    // Extract the partition key.
    string partitionKey = GetPartitionKey(context);

    return RateLimitPartition.GetConcurrencyLimiter(
        partitionKey,
        key => new ConcurrencyLimiterOptions
        {
            PermitLimit = 100
        });
});

new ResiliencePipelineBuilder()
    .AddRateLimiter(new RateLimiterStrategyOptions
    {
        // Provide a custom rate limiter delegate.
        RateLimiter = args =>
        {
            return partitionedLimiter.AcquireAsync(args.Context, 1, args.Context.CancellationToken);
        }
    });
```
<!-- endSnippet -->
