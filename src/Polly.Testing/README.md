# About Polly.Testing

This package exposes APIs and utilities that can be used to assert on the composition of resilience strategies.

``` csharp
// Build your resilience strategy.
ResilienceStrategy strategy = new CompositeStrategyBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        RetryCount = 4
    })
    .AddTimeout(TimeSpan.FromSeconds(1))
    .ConfigureTelemetry(NullLoggerFactory.Instance)
    .Build();

// Retrieve inner strategies.
InnerStrategiesDescriptor descriptor = strategy.GetInnerStrategies();

// Assert the composition.
Assert.True(descriptor.HasTelemetry);
Assert.Equal(2, descriptor.Strategies.Count);

var retryOptions = Assert.IsType<RetryStrategyOptions>(descriptor.Strategies[0]);
Assert.Equal(4, retryOptions.RetryCount);

var timeoutOptions = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[0]);
Assert.Equal(TimeSpan.FromSeconds(1), timeoutOptions.Timeout);
```
