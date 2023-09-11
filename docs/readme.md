# Polly Documentation

Polly is a .NET resilience and transient-fault-handling library that allows developers to express resilience strategies such as Retry, Circuit Breaker, Hedging, Timeout, Rate Limiter and Fallback in a fluent and thread-safe manner.

We are a member of the [.NET Foundation](https://www.dotnetfoundation.org/about)!

![Polly logo](https://raw.github.com/App-vNext/Polly/main/Polly-Logo.png)

## Quick start

To use Polly, you must provide a callback and execute it using [**resilience pipeline**](resilience-pipelines.md). A resilience pipeline is a combination of one or more [**resilience strategies**](resilience-strategies.md) such as retry, timeout, and rate limiter. Polly uses **builders** to integrate these strategies into a pipeline.

To get started, first add the [Polly.Core](https://www.nuget.org/packages/Polly.Core/) package to your project by running the following command:

```sh
dotnet add package Polly.Core
```

You can create a `ResiliencePipeline` using the `ResiliencePipelineBuilder` class as shown below:

<!-- snippet: quick-start -->
```cs
// Create a instance of builder that exposes various extensions for adding resilience strategies
var builder = new ResiliencePipelineBuilder();

// Add retry using the default options
builder.AddRetry(new RetryStrategyOptions());

// Add 10 second timeout
builder.AddTimeout(TimeSpan.FromSeconds(10));

// Build the resilience pipeline
ResiliencePipeline pipeline = builder.Build();

// Execute the pipeline
await pipeline.ExecuteAsync(async token =>
{
    // Your custom logic here
});
```
<!-- endSnippet -->

### Dependency injection

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

## Resilience strategies

| Strategy | Reactive | Premise | AKA | How does the strategy mitigate?|
| ------------- | --- | ------------- |:-------------: |------------- |
|[Retry](strategies/retry.md) |Yes|Many faults are transient and may self-correct after a short delay.| *Maybe it's just a blip* |  Allows configuring automatic retries. |
|[Circuit-breaker](strategies/circuit-breaker.md) |Yes|When a system is seriously struggling, failing fast is better than making users/callers wait.  <br/><br/>Protecting a faulting system from overload can help it recover. | *Stop doing it if it hurts* <br/><br/>*Give that system a break* | Breaks the circuit (blocks executions) for a period, when faults exceed some pre-configured threshold. |
|[Timeout](strategies/timeout.md)|No|Beyond a certain wait, a success result is unlikely.| *Don't wait forever*  |Guarantees the caller won't have to wait beyond the timeout. |
|[Rate Limiter](strategies/rate-limiter.md)|No|Limiting the rate a system handles requests is another way to control load. <br/><br/> This can apply to the way your system accepts incoming calls, and/or to the way you call downstream services. | *Slow down a bit, will you?*  |Constrains executions to not exceed a certain rate. |
|[Fallback](strategies/fallback.md)|Yes|Things will still fail - plan what you will do when that happens.| *Degrade gracefully*  |Defines an alternative value to be returned (or action to be executed) on failure. |
|[Hedging](strategies/hedging.md)|Yes|Things can be slow sometimes, plan what you will do when that happens.| *Hedge your bets*  | Executes parallel actions when things are slow and waits for the fastest one.  |

Visit the [resilience strategies](resilience-strategies.md) section to understand their structure and explore various configuration methods.

## Articles

- [General](general.md): General information about Polly.
- [Resilience pipelines](resilience-pipelines.md): Understanding the use of resilience pipelines.
- [Resilience strategies](resilience-strategies.md): General information about resilience strategies and how to configure them.
- [Resilience context](resilience-context.md): Describes the resilience context used by resilience pipelines and strategies.
- [Resilience pipeline registry](resilience-pipeline-registry.md): Exploring the registry that stores resilience pipelines.
- [Dependency injection](dependency-injection.md): How Polly integrates with Dependency Injection.
- [Telemetry](telemetry.md): Insights into telemetry generated by resilience pipelines and strategies.
- [Extensibility](v7/extensibility.md): Learn how you can extend Polly with new resilience strategies.
- [Polly-Contrib](polly-contrib.md): Learn how to contribute to and extend the Polly ecosystem.
- [Simmy](simmy.md): Get to know chaos engineering via Polly's capabilities.
- [Third-Party Libraries and Contributions](libraries-and-contributions.md): Find out which libraries Polly depends on and who contributes to its development.
- [Resources](resources.md): Browse through blogs, podcasts, courses, e-books, and other community resources.
- [Using Polly with HttpClientFactory](https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory): For using Polly with HttpClientFactory in ASP.NET Core 2.1 and later versions.

## Articles (previous Polly versions)

- [Extensibility (v7)](v7/extensibility.md): Learn how you can extend Polly with new policies.

## Samples

- [Samples](samples/README.md): Samples in this repository that serve as an introduction to Polly.
- [Polly-Samples](https://github.com/App-vNext/Polly-Samples): Contains practical examples for using various implementations of Polly. Please feel free to contribute to the Polly-Samples repository in order to assist others who are either learning Polly for the first time, or are seeking advanced examples and novel approaches provided by our generous community.
- Microsoft's [eShopOnContainers project](https://github.com/dotnet-architecture/eShopOnContainers): Sample project demonstrating a .NET Microservices architecture and using Polly for resilience.
