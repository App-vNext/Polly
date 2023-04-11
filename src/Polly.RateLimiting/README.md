# About Polly.Hosting

The `Polly.RateLimiting` adopts the [.NET Rate Limiting](https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/) APIs for Polly scenarios.

- It exposes the `AddRateLimiter` extension methods for `ResiliencePipelineBuilder`.
- It exposes the `AddConcurrencyLimiter` convenience extension methods for `ResiliencePipelineBuilder`.
- It exposes the `RateLimiterRejectedException` to notify the caller that the operation was rate limited.

Example:

``` csharp
// Convenience extension method for ConcurrencyLimiter
builder.AddConcurrencyLimiter(
    new ConcurrencyLimiterOptions
    {
        PermitLimit = 10,
        QueueLimit = 10
    },
    () => Console.WriteLine("Rate limiter rejected!"));

// Convenience extension method
builder.AddRateLimiter(
    new ConcurrencyLimiter(new ConcurrencyLimiterOptions
    {
        PermitLimit = 10,
        QueueLimit = 10
    }),
    onRejected => onRejected.Add(() => Console.WriteLine("Rate limiter rejected!")));

// Add rate limiter using the RateLimiterStrategyOptions
builder.AddRateLimiter(new RateLimiterStrategyOptions
{
    RateLimiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
    {
        PermitLimit = 10,
        QueueLimit = 10
    }),
    OnRejected = new OnRateLimiterRejectedEvent().Add(() => Console.WriteLine("Rate limiter rejected!"))
});
```

