# Polly

Polly is a .NET resilience and transient-fault-handling library that allows developers to express resilience strategies such as Retry, Circuit Breaker, Hedging, Timeout, Rate Limiter and Fallback in a fluent and thread-safe manner.

[<img align="right" src="https://github.com/dotnet/swag/raw/main/logo/dotnetfoundation_v4_small.png" width="100" alt="The .NET Foundation logo" />](https://www.dotnetfoundation.org/)
We are a member of the [.NET Foundation](https://www.dotnetfoundation.org/about)!

**Keep up to date with new feature announcements, tips & tricks, and other news through [www.thepollyproject.org](https://www.thepollyproject.org)**

[![Build status](https://github.com/App-vNext/Polly/workflows/build/badge.svg?branch=main&event=push)](https://github.com/App-vNext/Polly/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush) [![Code coverage](https://codecov.io/gh/App-vNext/Polly/branch/main/graph/badge.svg)](https://codecov.io/gh/App-vNext/Polly) [![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/App-vNext/Polly/badge)](https://securityscorecards.dev/viewer/?uri=github.com/App-vNext/Polly)

![Polly logo](https://raw.github.com/App-vNext/Polly/main/Polly-Logo.png)

> [!IMPORTANT]
> This documentation describes the new Polly v8 API. If you are using the v7 API, please refer to the [previous version](https://github.com/App-vNext/Polly/tree/7.2.4) of the documentation.

## NuGet Packages

| **Package** | **Latest Version** | **About** |
|:--|:--|:--|
| `Polly.Core` | [![NuGet](https://buildstats.info/nuget/Polly.Core)](https://www.nuget.org/packages/Polly.Core/ "Download Polly.Core from NuGet.org") | The core abstractions and [built-in strategies](https://www.pollydocs.org/strategies/index). |
| `Polly.Extensions` | [![NuGet](https://buildstats.info/nuget/Polly.Extensions)](https://www.nuget.org/packages/Polly.Extensions/ "Download Polly.Extensions from NuGet.org") | [Telemetry](https://www.pollydocs.org/advanced/telemetry) and [dependency injection](https://www.pollydocs.org/advanced/dependency-injection) support. |
| `Polly.RateLimiting` | [![NuGet](https://buildstats.info/nuget/Polly.RateLimiting)](https://www.nuget.org/packages/Polly.RateLimiting/ "Download Polly.RateLimiting from NuGet.org") | Integration with [`System.Threading.RateLimiting`](https://www.nuget.org/packages/System.Threading.RateLimiting) APIs. |
| `Polly.Testing` | [![NuGet](https://buildstats.info/nuget/Polly.Testing)](https://www.nuget.org/packages/Polly.Testing/ "Download Polly.Testing from NuGet.org") | [Testing support](https://www.pollydocs.org/advanced/testing) for Polly libraries. |
| `Polly` | [![NuGet](https://buildstats.info/nuget/Polly)](https://www.nuget.org/packages/Polly/ "Download Polly from NuGet.org") | This package contains the legacy API exposed by versions of the Polly library before version 8. |

## Documentation

This README aims to give a quick overview of some Polly features - including enough to get you started with any resilience strategy. For deeper detail on any resilience strategy, and many other aspects of Polly, be sure also to check out [pollydocs.org][polly-docs].

## Quick start

To use Polly, you must provide a callback and execute it using [**resilience pipeline**](https://www.pollydocs.org/pipelines). A resilience pipeline is a combination of one or more [**resilience strategies**](https://www.pollydocs.org/strategies) such as retry, timeout, and rate limiter. Polly uses **builders** to integrate these strategies into a pipeline.

To get started, first add the [Polly.Core](https://www.nuget.org/packages/Polly.Core/) package to your project by running the following command:

```sh
dotnet add package Polly.Core
```

You can create a `ResiliencePipeline` using the `ResiliencePipelineBuilder` class as shown below:

<!-- snippet: quick-start -->
```cs
// Create an instance of builder that exposes various extensions for adding resilience strategies
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions()) // Add retry using the default options
    .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 seconds timeout
    .Build(); // Builds the resilience pipeline

// Execute the pipeline asynchronously
await pipeline.ExecuteAsync(static async token => { /* Your custom logic goes here */ }, cancellationToken);
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
var serviceProvider = services.BuildServiceProvider();

// Retrieve a ResiliencePipelineProvider that dynamically creates and caches the resilience pipelines
var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

// Retrieve your resilience pipeline using the name it was registered with
ResiliencePipeline pipeline = pipelineProvider.GetPipeline("my-pipeline");

// Alternatively, you can use keyed services to retrieve the resilience pipeline
pipeline = serviceProvider.GetRequiredKeyedService<ResiliencePipeline>("my-pipeline");

// Execute the pipeline
await pipeline.ExecuteAsync(static async token =>
{
    // Your custom logic goes here
});
```
<!-- endSnippet -->

## Resilience strategies

Polly provides a variety of resilience strategies. Alongside the comprehensive guides for each strategy, the wiki also includes an [overview of the role each strategy plays in resilience engineering](https://github.com/App-vNext/Polly/wiki/Transient-fault-handling-and-proactive-resilience-engineering).

Polly categorizes resilience strategies into two main groups:

### Reactive

These strategies handle specific exceptions that are thrown, or results that are returned, by the callbacks executed through the strategy.

| Strategy | Premise | AKA | Mitigation |
| ------------- | ------------- | -------------- | ------------ |
|[**Retry** family](#retry) |Many faults are transient and may self-correct after a short delay.| *Maybe it's just a blip* |  Allows configuring automatic retries. |
|[**Circuit-breaker** family](#circuit-breaker)|When a system is seriously struggling, failing fast is better than making users/callers wait.  <br/><br/>Protecting a faulting system from overload can help it recover. | *Stop doing it if it hurts* <br/><br/>*Give that system a break* | Breaks the circuit (blocks executions) for a period, when faults exceed some pre-configured threshold. |
|[**Fallback**](#fallback)|Things will still fail - plan what you will do when that happens.| *Degrade gracefully*  |Defines an alternative value to be returned (or action to be executed) on failure. |
|[**Hedging**](#hedging)|Things can be slow sometimes, plan what you will do when that happens.| *Hedge your bets*  | Executes parallel actions when things are slow and waits for the fastest one.  |

### Proactive

Unlike reactive strategies, proactive strategies do not focus on handling errors by the callbacks might throw or return. They can make pro-active decisions to cancel or reject the execution of callbacks.

| Strategy | Premise | AKA | Prevention |
| ----------- | ------------- | -------------- | ------------ |
|[**Timeout**](#timeout)|Beyond a certain wait, a success result is unlikely.| *Don't wait forever*  |Guarantees the caller won't have to wait beyond the timeout. |
|[**Rate Limiter**](#rate-limiter)|Limiting the rate a system handles requests is another way to control load. <br/> <br/> This can apply to the way your system accepts incoming calls, and/or to the way you call downstream services. | *Slow down a bit, will you?*  |Constrains executions to not exceed a certain rate. |

Visit [resilience strategies](https://www.pollydocs.org/strategies) docs to explore how to configure individual resilience strategies in more detail.

### Retry

<!-- snippet: retry -->
```cs
// Retry using the default options.
// See https://www.pollydocs.org/strategies/retry#defaults for defaults.
var optionsDefaults = new RetryStrategyOptions();

// For instant retries with no delay
var optionsNoDelay = new RetryStrategyOptions
{
    Delay = TimeSpan.Zero
};

// For advanced control over the retry behavior, including the number of attempts,
// delay between retries, and the types of exceptions to handle.
var optionsComplex = new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true,  // Adds a random factor to the delay
    MaxRetryAttempts = 4,
    Delay = TimeSpan.FromSeconds(3),
};

// To use a custom function to generate the delay for retries
var optionsDelayGenerator = new RetryStrategyOptions
{
    MaxRetryAttempts = 2,
    DelayGenerator = static args =>
    {
        var delay = args.AttemptNumber switch
        {
            0 => TimeSpan.Zero,
            1 => TimeSpan.FromSeconds(1),
            _ => TimeSpan.FromSeconds(5)
        };

        // This example uses a synchronous delay generator,
        // but the API also supports asynchronous implementations.
        return new ValueTask<TimeSpan?>(delay);
    }
};

// To extract the delay from the result object
var optionsExtractDelay = new RetryStrategyOptions<HttpResponseMessage>
{
    DelayGenerator = static args =>
    {
        if (args.Outcome.Result is HttpResponseMessage responseMessage &&
            TryGetDelay(responseMessage, out TimeSpan delay))
        {
            return new ValueTask<TimeSpan?>(delay);
        }

        // Returning null means the retry strategy will use its internal delay for this attempt.
        return new ValueTask<TimeSpan?>((TimeSpan?)null);
    }
};

// To get notifications when a retry is performed
var optionsOnRetry = new RetryStrategyOptions
{
    MaxRetryAttempts = 2,
    OnRetry = static args =>
    {
        Console.WriteLine("OnRetry, Attempt: {0}", args.AttemptNumber);

        // Event handlers can be asynchronous; here, we return an empty ValueTask.
        return default;
    }
};

// To keep retrying indefinitely or until success use int.MaxValue.
var optionsIndefiniteRetry = new RetryStrategyOptions
{
    MaxRetryAttempts = int.MaxValue,
};

// Add a retry strategy with a RetryStrategyOptions{<TResult>} instance to the pipeline
new ResiliencePipelineBuilder().AddRetry(optionsDefaults);
new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(optionsExtractDelay);
```
<!-- endSnippet -->

If all retries fail, a retry strategy rethrows the final exception back to the calling code.

For more details, visit the [retry strategy](https://www.pollydocs.org/strategies/retry) documentation.

### Circuit Breaker

<!-- snippet: circuit-breaker -->
```cs
// Circuit breaker with default options.
// See https://www.pollydocs.org/strategies/circuit-breaker#defaults for defaults.
var optionsDefaults = new CircuitBreakerStrategyOptions();

// Circuit breaker with customized options:
// The circuit will break if more than 50% of actions result in handled exceptions,
// within any 10-second sampling duration, and at least 8 actions are processed.
var optionsComplex = new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,
    SamplingDuration = TimeSpan.FromSeconds(10),
    MinimumThroughput = 8,
    BreakDuration = TimeSpan.FromSeconds(30),
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>()
};

// Circuit breaker using BreakDurationGenerator:
// The break duration is dynamically determined based on the properties of BreakDurationGeneratorArguments.
var optionsBreakDurationGenerator = new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,
    SamplingDuration = TimeSpan.FromSeconds(10),
    MinimumThroughput = 8,
    BreakDurationGenerator = static args => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(args.FailureCount)),
};

// Handle specific failed results for HttpResponseMessage:
var optionsShouldHandle = new CircuitBreakerStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<SomeExceptionType>()
        .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
};

// Monitor the circuit state, useful for health reporting:
var stateProvider = new CircuitBreakerStateProvider();
var optionsStateProvider = new CircuitBreakerStrategyOptions<HttpResponseMessage>
{
    StateProvider = stateProvider
};

var circuitState = stateProvider.CircuitState;

/*
CircuitState.Closed - Normal operation; actions are executed.
CircuitState.Open - Circuit is open; actions are blocked.
CircuitState.HalfOpen - Recovery state after break duration expires; actions are permitted.
CircuitState.Isolated - Circuit is manually held open; actions are blocked.
*/

// Manually control the Circuit Breaker state:
var manualControl = new CircuitBreakerManualControl();
var optionsManualControl = new CircuitBreakerStrategyOptions
{
    ManualControl = manualControl
};

// Manually isolate a circuit, e.g., to isolate a downstream service.
await manualControl.IsolateAsync();

// Manually close the circuit to allow actions to be executed again.
await manualControl.CloseAsync();

// Add a circuit breaker strategy with a CircuitBreakerStrategyOptions{<TResult>} instance to the pipeline
new ResiliencePipelineBuilder().AddCircuitBreaker(optionsDefaults);
new ResiliencePipelineBuilder<HttpResponseMessage>().AddCircuitBreaker(optionsStateProvider);
```
<!-- endSnippet -->

For more details, visit the [circuit breaker strategy](https://www.pollydocs.org/strategies/circuit-breaker) documentation.

### Fallback

<!-- snippet: fallback -->
```cs
// A fallback/substitute value if an operation fails.
var optionsSubstitute = new FallbackStrategyOptions<UserAvatar>
{
    ShouldHandle = new PredicateBuilder<UserAvatar>()
        .Handle<SomeExceptionType>()
        .HandleResult(r => r is null),
    FallbackAction = static args => Outcome.FromResultAsValueTask(UserAvatar.Blank)
};

// Use a dynamically generated value if an operation fails.
var optionsFallbackAction = new FallbackStrategyOptions<UserAvatar>
{
    ShouldHandle = new PredicateBuilder<UserAvatar>()
        .Handle<SomeExceptionType>()
        .HandleResult(r => r is null),
    FallbackAction = static args =>
    {
        var avatar = UserAvatar.GetRandomAvatar();
        return Outcome.FromResultAsValueTask(avatar);
    }
};

// Use a default or dynamically generated value, and execute an additional action if the fallback is triggered.
var optionsOnFallback = new FallbackStrategyOptions<UserAvatar>
{
    ShouldHandle = new PredicateBuilder<UserAvatar>()
        .Handle<SomeExceptionType>()
        .HandleResult(r => r is null),
    FallbackAction = static args =>
    {
        var avatar = UserAvatar.GetRandomAvatar();
        return Outcome.FromResultAsValueTask(UserAvatar.Blank);
    },
    OnFallback = static args =>
    {
        // Add extra logic to be executed when the fallback is triggered, such as logging.
        return default; // Returns an empty ValueTask
    }
};

// Add a fallback strategy with a FallbackStrategyOptions<TResult> instance to the pipeline
new ResiliencePipelineBuilder<UserAvatar>().AddFallback(optionsOnFallback);
```
<!-- endSnippet -->

For more details, visit the [fallback strategy](https://www.pollydocs.org/strategies/fallback) documentation.

### Hedging

<!-- snippet: hedging -->
```cs
// Hedging with default options.
// See https://www.pollydocs.org/strategies/hedging#defaults for defaults.
var optionsDefaults = new HedgingStrategyOptions<HttpResponseMessage>();

// A customized hedging strategy that retries up to 3 times if the execution
// takes longer than 1 second or if it fails due to an exception or returns an HTTP 500 Internal Server Error.
var optionsComplex = new HedgingStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<SomeExceptionType>()
        .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError),
    MaxHedgedAttempts = 3,
    Delay = TimeSpan.FromSeconds(1),
    ActionGenerator = static args =>
    {
        Console.WriteLine("Preparing to execute hedged action.");

        // Return a delegate function to invoke the original action with the action context.
        // Optionally, you can also create a completely new action to be executed.
        return () => args.Callback(args.ActionContext);
    }
};

// Subscribe to hedging events.
var optionsOnHedging = new HedgingStrategyOptions<HttpResponseMessage>
{
    OnHedging = static args =>
    {
        Console.WriteLine($"OnHedging: Attempt number {args.AttemptNumber}");
        return default;
    }
};

// Add a hedging strategy with a HedgingStrategyOptions<TResult> instance to the pipeline
new ResiliencePipelineBuilder<HttpResponseMessage>().AddHedging(optionsDefaults);
```
<!-- endSnippet -->

If all hedged attempts fail, the hedging strategy will either re-throw the original exception or return the original failed result to the caller.

For more details, visit the [hedging strategy](https://www.pollydocs.org/strategies/hedging) documentation.

### Timeout

The timeout resilience strategy assumes delegates you execute support [co-operative cancellation](https://learn.microsoft.com/dotnet/standard/threading/cancellation-in-managed-threads). You must use `Execute/Async(...)` overloads taking a `CancellationToken`, and the executed delegate must honor that `CancellationToken`.

<!-- snippet: timeout -->
```cs
// To add a timeout with a custom TimeSpan duration
new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromSeconds(3));

// Timeout using the default options.
// See https://www.pollydocs.org/strategies/timeout#defaults for defaults.
var optionsDefaults = new TimeoutStrategyOptions();

// To add a timeout using a custom timeout generator function
var optionsTimeoutGenerator = new TimeoutStrategyOptions
{
    TimeoutGenerator = static args =>
    {
        // Note: the timeout generator supports asynchronous operations
        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
    }
};

// To add a timeout and listen for timeout events
var optionsOnTimeout = new TimeoutStrategyOptions
{
    TimeoutGenerator = static args =>
    {
        // Note: the timeout generator supports asynchronous operations
        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
    },
    OnTimeout = static args =>
    {
        Console.WriteLine($"{args.Context.OperationKey}: Execution timed out after {args.Timeout.TotalSeconds} seconds.");
        return default;
    }
};

// Add a timeout strategy with a TimeoutStrategyOptions instance to the pipeline
new ResiliencePipelineBuilder().AddTimeout(optionsDefaults);
```
<!-- endSnippet -->

Timeout strategies throw `TimeoutRejectedException` when a timeout occurs.

For more details, visit the [timeout strategy](https://www.pollydocs.org/strategies/timeout) documentation.

### Rate Limiter

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
            Window = TimeSpan.FromMinutes(1)
        }));
```
<!-- endSnippet -->

Rate limiter strategy throws `RateLimiterRejectedException` if execution is rejected.

For more details, visit the [rate limiter strategy](https://www.pollydocs.org/strategies/rate-limiter) documentation.

## Chaos engineering

Starting with version `8.3.0`, Polly has integrated [Simmy](https://github.com/Polly-Contrib/Simmy), a chaos engineering library, directly into its core. For more information, please refer to the dedicated [chaos engineering documentation](https://www.pollydocs.org/chaos/).

## Next steps

To learn more about Polly, visit [pollydocs.org][polly-docs].

## Samples

- [Samples](samples/README.md): Samples in this repository that serve as an introduction to Polly.
- [Polly-Samples](https://github.com/App-vNext/Polly-Samples): Contains practical examples for using various implementations of Polly. Please feel free to contribute to the Polly-Samples repository in order to assist others who are either learning Polly for the first time, or are seeking advanced examples and novel approaches provided by our generous community.
- Microsoft's [eShopOnContainers project](https://github.com/dotnet-architecture/eShopOnContainers): Sample project demonstrating a .NET Micro-services architecture and using Polly for resilience.

### Sponsors

Thanks to the [.NET on AWS Open Source Software Fund](https://github.com/aws/dotnet-foss) for sponsoring the ongoing development of Polly.

![AWS logo](./logos/aws.png)

Help support this project by becoming a sponsor through [GitHub Sponsors](https://github.com/sponsors/martincostello).

## License

Licensed under the terms of the [New BSD License](https://opensource.org/license/bsd-3-clause/)

[polly-docs]: https://www.pollydocs.org/
