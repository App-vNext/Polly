# About Polly.RateLimiting

The `Polly.RateLimiting` package adopts the [.NET Rate Limiting](https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/) APIs for Polly scenarios.

- It exposes the `AddRateLimiter` extension methods for `ResiliencePipelineBuilder`.
- It exposes the `AddConcurrencyLimiter` convenience extension methods for `ResiliencePipelineBuilder`.
- It exposes the `RateLimiterRejectedException` class to notify the caller that the operation was rate limited.

Example:

<!-- snippet: rate-limiter-usage -->
```cs
ResiliencePipelineBuilder builder = new ResiliencePipelineBuilder();

// Convenience extension method for ConcurrencyLimiter
builder.AddConcurrencyLimiter(permitLimit: 10, queueLimit: 10);

// Convenience extension method for ConcurrencyLimiter with callback
builder.AddConcurrencyLimiter(
    new ConcurrencyLimiterOptions
    {
        PermitLimit = 10,
        QueueLimit = 10
    });

// Convenience extension method with custom limiter creation
builder.AddRateLimiter(
    new ConcurrencyLimiter(new ConcurrencyLimiterOptions
    {
        PermitLimit = 10,
        QueueLimit = 10
    }));

// Add rate limiter using the RateLimiterStrategyOptions
var limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
{
    PermitLimit = 10,
    QueueLimit = 10
});

builder.AddRateLimiter(new RateLimiterStrategyOptions
{
    RateLimiter = args => limiter.AcquireAsync(1, args.Context.CancellationToken),
    OnRejected = _ =>
    {
        Console.WriteLine("Rate limiter rejected!");
        return default;
    }
});
```
<!-- endSnippet -->
