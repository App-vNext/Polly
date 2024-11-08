# Performance

Polly is fast and avoids allocations wherever possible. We use a comprehensive set of [performance benchmarks](https://github.com/App-vNext/Polly/tree/main/bench/Polly.Core.Benchmarks) to monitor Polly's performance.

Here's an example of results from an advanced pipeline composed of the following strategies:

- Timeout (outer)
- Rate limiter
- Retry
- Circuit breaker
- Timeout (inner)

---

| Method              |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
| ------------------- | -------: | --------: | --------: | ----: | ------: | -----: | --------: | ----------: |
| Execute policy v7   | 2.277 μs | 0.0133 μs | 0.0191 μs |  1.00 |    0.00 | 0.1106 |    2824 B |        1.00 |
| Execute pipeline v8 | 2.089 μs | 0.0105 μs | 0.0157 μs |  0.92 |    0.01 |      - |      40 B |        0.01 |

---

Compared to older versions, Polly v8 is both faster and more memory efficient.

## Performance tips

If you're aiming for the best performance with Polly, consider these tips:

### Use static lambdas

Lambdas capturing variables from their outer scope will allocate on every execution. Polly provides tools to avoid this overhead, as shown in the example below:

<!-- snippet: perf-lambdas -->
```cs
// This call allocates for each invocation since the "userId" variable is captured from the outer scope.
await resiliencePipeline.ExecuteAsync(
    cancellationToken => GetMemberAsync(userId, cancellationToken),
    cancellationToken);

// This approach uses a static lambda, avoiding allocations.
// The "userId" is passed to the execution via the state argument, and the lambda consumes it as the first
// parameter passed to the GetMemberAsync() method. In this case, userIdAsState and userId are the same value.
await resiliencePipeline.ExecuteAsync(
    static (userIdAsState, cancellationToken) => GetMemberAsync(userIdAsState, cancellationToken),
    userId,
    cancellationToken);
```
<!-- endSnippet -->

### Use switch expressions for predicates

The `PredicateBuilder` maintains a list of all registered predicates. To determine whether the results should be processed, it iterates through this list. Using switch expressions can help you bypass this overhead.

<!-- snippet: perf-switch-expressions -->
```cs
// Here, PredicateBuilder is used to configure which exceptions the retry strategy should handle.
new ResiliencePipelineBuilder()
    .AddRetry(new()
    {
        ShouldHandle = new PredicateBuilder()
            .Handle<SomeExceptionType>()
            .Handle<InvalidOperationException>()
            .Handle<HttpRequestException>()
    })
    .Build();

// For optimal performance, it's recommended to use switch expressions instead of PredicateBuilder.
new ResiliencePipelineBuilder()
    .AddRetry(new()
    {
        ShouldHandle = args => args.Outcome.Exception switch
        {
            SomeExceptionType => PredicateResult.True(),
            InvalidOperationException => PredicateResult.True(),
            HttpRequestException => PredicateResult.True(),
            _ => PredicateResult.False()
        }
    })
    .Build();
```
<!-- endSnippet -->

### Execute callbacks without throwing exceptions

Polly provides the `ExecuteOutcomeAsync` API, returning results as `Outcome<T>`. The `Outcome<T>` might contain an exception instance, which you can check without it being thrown. This is beneficial when employing exception-heavy resilience strategies, like circuit breakers.

<!-- snippet: perf-execute-outcome -->
```cs
// Execute GetMemberAsync and handle exceptions externally.
try
{
    await pipeline.ExecuteAsync(cancellationToken => GetMemberAsync(id, cancellationToken), cancellationToken);
}
catch (Exception e)
{
    // Log the exception here.
    logger.LogWarning(e, "Failed to get member with id '{id}'.", id);
}

// The example above can be restructured as:

// Acquire a context from the pool
ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);

// Instead of wrapping pipeline execution with try-catch, use ExecuteOutcomeAsync(...).
// Certain strategies are optimized for this method, returning an exception instance without actually throwing it.
Outcome<Member> outcome = await pipeline.ExecuteOutcomeAsync(
    static async (context, state) =>
    {
        // The callback for ExecuteOutcomeAsync must return an Outcome<T> instance. Hence, some wrapping is needed.
        try
        {
            return Outcome.FromResult(await GetMemberAsync(state, context.CancellationToken));
        }
        catch (Exception e)
        {
            return Outcome.FromException<Member>(e);
        }
    },
    context,
    id);

// Handle exceptions using the Outcome<T> instance instead of try-catch.
if (outcome.Exception is not null)
{
    logger.LogWarning(outcome.Exception, "Failed to get member with id '{id}'.", id);
}

// Release the context back to the pool
ResilienceContextPool.Shared.Return(context);
```
<!-- endSnippet -->

### Reuse resilience pipeline instances

Creating a resilience pipeline can be resource-intensive, so it's advisable not to discard the instance after each use. Instead, you can either cache the resilience pipeline or use the `GetOrAddPipeline(...)` method from `ResiliencePipelineRegistry<T>` to cache the pipeline dynamically:

<!-- snippet: perf-reuse-pipelines -->
```cs
public sealed class MyApi
{
    private readonly ResiliencePipelineRegistry<string> _registry;

    // Share a single instance of the registry throughout your application.
    public MyApi(ResiliencePipelineRegistry<string> registry)
    {
        _registry = registry;
    }

    public async Task UpdateData(CancellationToken cancellationToken)
    {
        // Get or create the pipeline, and then cache it for subsequent use.
        // Choose a sufficiently unique key to prevent collisions.
        var pipeline = _registry.GetOrAddPipeline("my-app.my-api", builder =>
        {
            builder.AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<InvalidOperationException>()
                    .Handle<HttpRequestException>()
            });
        });

        await pipeline.ExecuteAsync(async token =>
        {
            // Place your logic here
        },
        cancellationToken);
    }
}
```
<!-- endSnippet -->

> [!NOTE]
> You can also define your pipeline on startup using [dependency injection](dependency-injection.md#usage) and then use `ResiliencePipelineProvider<T>` to retrieve the instance.
