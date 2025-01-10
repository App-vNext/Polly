# About Polly.RateLimiting

The `Polly.RateLimiting` package adopts the [.NET Rate Limiting](https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/) APIs for Polly scenarios.

- It exposes the `AddRateLimiter` extension methods for `ResiliencePipelineBuilder`.
- It exposes the `AddConcurrencyLimiter` convenience extension methods for `ResiliencePipelineBuilder`.
- It exposes the `RateLimiterRejectedException` class to notify the caller that the operation was rate limited.

See [the documentation](https://www.pollydocs.org/strategies/rate-limiter) for more details.

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
    .AddRateLimiter(new SlidingWindowRateLimiter(
        new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            SegmentsPerWindow = 4,
            Window = TimeSpan.FromMinutes(1)
        }));
```
<!-- endSnippet -->
