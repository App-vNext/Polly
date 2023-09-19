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
// The "userId" is stored as state, and the lambda reads it.
await resiliencePipeline.ExecuteAsync(
    static (state, cancellationToken) => GetMemberAsync(state, cancellationToken),
    userId,
    cancellationToken);
```
<!-- endSnippet -->

### Use switch expressions for predicates

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

// For optimal performance, it's recommended to use switch expressions over PredicateBuilder.
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
