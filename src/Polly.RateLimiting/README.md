# About Polly.RateLimiting

The `Polly.RateLimiting` package adopts the [.NET Rate Limiting](https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/) APIs for Polly scenarios.

- It exposes the `AddRateLimiter` extension methods for `ResiliencePipelineBuilder`.
- It exposes the `AddConcurrencyLimiter` convenience extension methods for `ResiliencePipelineBuilder`.
- It exposes the `RateLimiterRejectedException` class to notify the caller that the operation was rate limited.

Example:

``` csharp
// Convenience extension method for ConcurrencyLimiter
builder.AddConcurrencyLimiter(permitLimit: 10, queueLimit: 10);

// Convenience extension method for ConcurrencyLimiter with callback
builder.AddConcurrencyLimiter(
    new ConcurrencyLimiter(new ConcurrencyLimiterOptions
    {
        PermitLimit = 10,
        QueueLimit = 10
    }),
    args => Console.WriteLine("Rate limiter rejected!"));

// Convenience extension method with custom limiter creation
builder.AddRateLimiter(
    new ConcurrencyLimiter(new ConcurrencyLimiterOptions
    {
        PermitLimit = 10,
        QueueLimit = 10
    }),
    args => Console.WriteLine("Rate limiter rejected!"));
    
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

