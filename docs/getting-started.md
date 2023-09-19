# Getting started

To use Polly, you must provide a callback and execute it using [**resilience pipeline**](pipelines/index.md). A resilience pipeline is a combination of one or more [**resilience strategies**](strategies/index.md) such as retry, timeout, and rate limiter. Polly uses **builders** to integrate these strategies into a pipeline.

To get started, first add the [Polly.Core](https://www.nuget.org/packages/Polly.Core/) package to your project by running the following command:

```sh
dotnet add package Polly.Core
```

You can create a `ResiliencePipeline` using the `ResiliencePipelineBuilder` class as shown below:

<!-- snippet: quick-start -->
```cs
// Create a instance of builder that exposes various extensions for adding resilience strategies
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions()) // Add retry using the default options
    .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 second timeout
    .Build(); // Builds the resilience pipeline

// Execute the pipeline asynchronously
await pipeline.ExecuteAsync(async cancellationToken => { /*Your custom logic here */ }, cancellationToken);
```
<!-- endSnippet -->

## Dependency injection

If you prefer to define resilience pipelines using [`IServiceCollection`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection), you'll need to install the [Polly.Extensions](https://www.nuget.org/packages/Polly.Extensions/) package:

```sh
dotnet add package Polly.Extensions
```

You can then define your resilience pipeline using the `AddResiliencePipeline(...)` extension method as shown:

<!-- snippet: quick-start-di -->
```cs
var services = new ServiceCollection();

// Define a resilience pipeline with the name "my-pipeline"
services.AddResiliencePipeline("my-pipeline", builder =>
{
    builder
        .AddRetry(new RetryStrategyOptions())
        .AddTimeout(TimeSpan.FromSeconds(10));
});

// Build the service provider
IServiceProvider serviceProvider = services.BuildServiceProvider();

// Retrieve ResiliencePipelineProvider that caches and dynamically creates the resilience pipelines
var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

// Retrieve resilience pipeline using the name it was registered with
ResiliencePipeline pipeline = pipelineProvider.GetPipeline("my-pipeline");

// Execute the pipeline
await pipeline.ExecuteAsync(async token =>
{
    // Your custom logic here
});
```
<!-- endSnippet -->
