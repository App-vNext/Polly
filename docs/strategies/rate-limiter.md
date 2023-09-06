# Rate limiter resilience strategy

## About

- **Options**: [`RateLimiterStrategyOptions`](../../src/Polly.RateLimiting/RateLimiterStrategyOptions.cs)
- **Extensions**: `AddRateLimiter`, `AddConcurrencyLimiter`
- **Strategy Type**: Proactive
- **Exceptions**:
  - `RateLimiterRejectedException`: Thrown when a rate limiter rejects an execution.
- **Package**: [Polly.RateLimiting](https://www.nuget.org/packages/Polly.RateLimiting)

🚧 This documentation is being written as part of the Polly v8 release.

## Usage

<!-- snippet: rate-limiter -->
```cs
// Add rate limiter with default options.
// See https://github.com/App-vNext/Polly/blob/main/docs/strategies/rate-limiter.md#defaults for default values.
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

// Create a custom partitioned rate limiter.
var partitionedLimiter = PartitionedRateLimiter.Create<Polly.ResilienceContext, string>(context =>
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
| `RateLimiter`               | `Null`                                               | Generator that creates `RateLimitLease` for executions.                                         |
| `DefaultRateLimiterOptions` | `PermitLimit` set to 1000 and `QueueLimit` set to 0. | The options for the default concurrency limiter that will be used when `RateLimiter` is `null`. |
| `OnRejected`                | `Null`                                               | Event that is raised when the execution is rejected by the rate limiter.                        |
