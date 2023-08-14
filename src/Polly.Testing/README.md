# About Polly.Testing

This package exposes APIs and utilities that can be used to assert on the composition of resilience pipelines.

``` csharp
// Build your resilience pipeline.
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        RetryCount = 4
    })
    .AddTimeout(TimeSpan.FromSeconds(1))
    .ConfigureTelemetry(NullLoggerFactory.Instance)
    .Build();

// Retrieve inner strategies.
ResiliencePipelineDescriptor descriptor = strategy.GetPipelineDescriptor();

// Assert the composition.
Assert.Equal(2, descriptor.Strategies.Count);

var retryOptions = Assert.IsType<RetryStrategyOptions>(descriptor.Strategies[0].Options);
Assert.Equal(4, retryOptions.RetryCount);

var timeoutOptions = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[0].Options);
Assert.Equal(TimeSpan.FromSeconds(1), timeoutOptions.Timeout);
```
