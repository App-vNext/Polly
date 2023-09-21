# Testing

This document explains how to test Polly’s resilience pipelines. You should not test how the resilience pipelines operate internally, but rather test your own settings or custom delegates.

To make the testing process simpler, Polly offers the [`Polly.Testing`](https://www.nuget.org/packages/Polly.Testing/) package. This package has a range of APIs designed to help you test the setup and combination of resilience pipelines in your user code.

## Usage

Begin by adding the `Polly.Testing` package to your test project:

```sh
dotnet add package Polly.Testing
```

Use the `GetPipelineDescriptor` extension method to get the `ResiliencePipelineDescriptor` which provides details on the pipeline's composition:

<!-- snippet: get-pipeline-descriptor -->
```cs
// Build your resilience pipeline.
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 4
    })
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();

// Retrieve the descriptor.
ResiliencePipelineDescriptor descriptor = pipeline.GetPipelineDescriptor();

// Check the pipeline's composition with the descriptor.
Assert.Equal(2, descriptor.Strategies.Count);

// Verify the retry settings.
var retryOptions = Assert.IsType<RetryStrategyOptions>(descriptor.Strategies[0].Options);
Assert.Equal(4, retryOptions.MaxRetryAttempts);

// Confirm the timeout settings.
var timeoutOptions = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[1].Options);
Assert.Equal(TimeSpan.FromSeconds(1), timeoutOptions.Timeout);
```
<!-- endSnippet -->

The `GetPipelineDescriptor` extension method is also available for the generic `ResiliencePipeline<T>`:

<!-- snippet: get-pipeline-descriptor-generic -->
```cs
// Construct your resilience pipeline.
ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>()
    .AddRetry(new RetryStrategyOptions<string>
    {
        MaxRetryAttempts = 4
    })
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();

// Obtain the descriptor.
ResiliencePipelineDescriptor descriptor = pipeline.GetPipelineDescriptor();

// Check the pipeline's composition with the descriptor.
// ...
```
<!-- endSnippet -->
