# Testing

This document explains how to test Polly's resilience pipelines. You should not test how the resilience pipelines operate internally, but rather test your own settings or custom delegates.

To make the testing process simpler, Polly offers the [`Polly.Testing`](https://www.nuget.org/packages/Polly.Testing/) package. This package has a range of APIs designed to help you test the setup and combination of resilience pipelines in your user code.

## Usage

Begin by adding the [`Polly.Testing`](https://www.nuget.org/packages/Polly.Testing) package to your test project:

```sh
dotnet add package Polly.Testing
```

Use the `GetPipelineDescriptor` extension method to get the [`ResiliencePipelineDescriptor`](xref:Polly.Testing.ResiliencePipelineDescriptor) which provides details on the pipeline's composition:

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

## Mocking `ResiliencePipelineProvider<TKey>`

Consider the following code that might resemble a part of your project:

<!-- snippet: testing-resilience-pipeline-provider-usage -->
```cs
// Represents an arbitrary API that needs resilience support
public class MyApi
{
    private readonly ResiliencePipeline _pipeline;

    // The value of pipelineProvider is injected via dependency injection
    public MyApi(ResiliencePipelineProvider<string> pipelineProvider)
    {
        _pipeline = pipelineProvider.GetPipeline("my-pipeline");
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _pipeline.ExecuteAsync(
            static async token =>
            {
                // Add your code here
            },
            cancellationToken);
    }
}

// Extensions to incorporate MyApi into dependency injection
public static class MyApiExtensions
{
    public static IServiceCollection AddMyApi(this IServiceCollection services)
    {
        return services
            .AddResiliencePipeline("my-pipeline", builder =>
            {
                builder.AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 4
                });
            })
            .AddSingleton<MyApi>();
    }
}
```
<!-- endSnippet -->

In the example above:

- The `MyApi` class is introduced, representing part of your application that requires resilience support.
- The `AddMyApi` extension method is also defined, which integrates `MyApi` into dependency injection (DI) and sets up the resilience pipeline it uses.

For unit tests, if you want to assess the behavior of `ExecuteAsync`, it might not be practical to rely on the entire pipeline, especially since it could slow down tests during failure scenario evaluations. Instead, it's recommended to mock the `ResiliencePipelineProvider<string>` and return an empty pipeline:

<!-- snippet: testing-resilience-pipeline-provider-mocking -->
```cs
ResiliencePipelineProvider<string> pipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();

// Mock the pipeline provider to return an empty pipeline for testing
pipelineProvider
    .GetPipeline("my-pipeline")
    .Returns(ResiliencePipeline.Empty);

// Use the mocked pipeline provider in your code
var api = new MyApi(pipelineProvider);

// You can now test the api
```
<!-- endSnippet -->

This example leverages the [`NSubstitute`](https://github.com/nsubstitute/NSubstitute) library to mock the pipeline provider.
