# About Polly.RateLimiting

The `Polly.RateLimiting` package adopts the [.NET Rate Limiting](https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/) APIs for Polly scenarios.

- It exposes the `AddRateLimiter` extension methods for `ResiliencePipelineBuilder`.
- It exposes the `AddConcurrencyLimiter` convenience extension methods for `ResiliencePipelineBuilder`.
- It exposes the `RateLimiterRejectedException` class to notify the caller that the operation was rate limited.

Example:

snippet: rate-limiter-usage
