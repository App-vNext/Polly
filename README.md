# Polly

Polly is a .NET resilience and transient-fault-handling library that allows developers to express resilience strategies such as Retry, Circuit Breaker, Hedging, Timeout, Rate Limiter and Fallback in a fluent and thread-safe manner.

[<img align="right" src="https://github.com/dotnet/swag/raw/main/logo/dotnetfoundation_v4_small.png" width="100" />](https://www.dotnetfoundation.org/)
We are a member of the [.NET Foundation](https://www.dotnetfoundation.org/about)!

**Keep up to date with new feature announcements, tips & tricks, and other news through [www.thepollyproject.org](https://www.thepollyproject.org)**

[![Build status](https://github.com/App-vNext/Polly/workflows/build/badge.svg?branch=main&event=push)](https://github.com/App-vNext/Polly/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush) [![Code coverage](https://codecov.io/gh/App-vNext/Polly/branch/main/graph/badge.svg)](https://codecov.io/gh/App-vNext/Polly) [![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/App-vNext/Polly/badge)](https://securityscorecards.dev/viewer/?uri=github.com/App-vNext/Polly)

![Polly logo](https://raw.github.com/App-vNext/Polly/main/Polly-Logo.png)

> [!IMPORTANT]
> This documentation describes the new Polly v8 API. If you are using the v7 API, please refer to the [previous version](https://github.com/App-vNext/Polly/tree/7.2.4) of the documentation.

## NuGet Packages

| **Package** | **Latest Version** |
|:--|:--|
| Polly | [![NuGet](https://buildstats.info/nuget/Polly)](https://www.nuget.org/packages/Polly/ "Download Polly from NuGet.org") |
| Polly.Core | [![NuGet](https://buildstats.info/nuget/Polly.Core)](https://www.nuget.org/packages/Polly.Core/ "Download Polly.Core from NuGet.org") |
| Polly.Extensions | [![NuGet](https://buildstats.info/nuget/Polly.Extensions)](https://www.nuget.org/packages/Polly.Extensions/ "Download Polly.Extensions from NuGet.org") |
| Polly.RateLimiting | [![NuGet](https://buildstats.info/nuget/Polly.RateLimiting)](https://www.nuget.org/packages/Polly.RateLimiting/ "Download Polly.RateLimiting from NuGet.org") |
| Polly.Testing | [![NuGet](https://buildstats.info/nuget/Polly.Testing)](https://www.nuget.org/packages/Polly.Testing/ "Download Polly.Testing from NuGet.org") |

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
// Create a instance of builder that exposes various extensions for adding resilience strategies
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions()) // Add retry using the default options
    .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 second timeout
    .Build(); // Builds the resilience pipeline

// Execute the pipeline asynchronously
await pipeline.ExecuteAsync(static async cancellationToken => { /*Your custom logic here */ }, cancellationToken);
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
await pipeline.ExecuteAsync(static async token =>
{
    // Your custom logic here
});
```
<!-- endSnippet -->

## Resilience strategies

Polly provides a variety of resilience strategies. Alongside the comprehensive guides for each strategy, the wiki also includes an [overview of the role each strategy plays in resilience engineering](https://github.com/App-vNext/Polly/wiki/Transient-fault-handling-and-proactive-resilience-engineering).

Polly categorizes resilience strategies into two main groups:

- **Reactive**: These strategies handle specific exceptions that are thrown, or results that are returned, by the callbacks executed through the strategy.
- **Proactive**: Unlike reactive strategies, proactive strategies do not focus on handling errors by the callbacks might throw or return. They can make pro-active decisions to cancel or reject the execution of callbacks (e.g., using a rate limiter or a timeout resilience strategy).

| Strategy | Reactive | Premise | AKA | How does the strategy mitigate?|
| ------------- | --- | ------------- |:-------------: |------------- |
|**Retry** <br/>(strategy family)<br/><sub>([quick-start](#retry)&nbsp;;&nbsp;[deep](https://www.pollydocs.org/strategies/retry))</sub> |Yes|Many faults are transient and may self-correct after a short delay.| *Maybe it's just a blip* |  Allows configuring automatic retries. |
|**Circuit-breaker**<br/>(strategy family)<br/><sub>([quick-start](#circuit-breaker)&nbsp;;&nbsp;[deep](https://www.pollydocs.org/strategies/circuit-breaker))</sub>|Yes|When a system is seriously struggling, failing fast is better than making users/callers wait.  <br/><br/>Protecting a faulting system from overload can help it recover. | *Stop doing it if it hurts* <br/><br/>*Give that system a break* | Breaks the circuit (blocks executions) for a period, when faults exceed some pre-configured threshold. |
|**Timeout**<br/><sub>([quick-start](#timeout)&nbsp;;&nbsp;[deep](https://www.pollydocs.org/strategies/timeout))</sub>|No|Beyond a certain wait, a success result is unlikely.| *Don't wait forever*  |Guarantees the caller won't have to wait beyond the timeout. |
|**Rate Limiter**<br/><sub>([quick-start](#rate-limiter)&nbsp;;&nbsp;[deep](https://www.pollydocs.org/strategies/rate-limiter))</sub>|No|Limiting the rate a system handles requests is another way to control load. <br/><br/> This can apply to the way your system accepts incoming calls, and/or to the way you call downstream services. | *Slow down a bit, will you?*  |Constrains executions to not exceed a certain rate. |
|**Fallback**<br/><sub>([quick-start](#fallback)&nbsp;;&nbsp;[deep](https://www.pollydocs.org/strategies/fallback))</sub>|Yes|Things will still fail - plan what you will do when that happens.| *Degrade gracefully*  |Defines an alternative value to be returned (or action to be executed) on failure. |
|**Hedging**<br/><sub>([quick-start](#hedging)&nbsp;;&nbsp;[deep](https://www.pollydocs.org/strategies/hedging))</sub>|Yes|Things can be slow sometimes, plan what you will do when that happens.| *Hedge your bets*  | Executes parallel actions when things are slow and waits for the fastest one.  |

Visit [resilience strategies](https://www.pollydocs.org/strategies) docs to explore how to configure individual resilience strategies in more detail.

### Retry

<!-- snippet: retry -->
```cs
// Add retry using the default options.
// See https://www.pollydocs.org/strategies/retry#defaults for defaults.
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions());

// For instant retries with no delay
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    Delay = TimeSpan.Zero
});

// For advanced control over the retry behavior, including the number of attempts,
// delay between retries, and the types of exceptions to handle.
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true,  // Adds a random factor to the delay
    MaxRetryAttempts = 4,
    Delay = TimeSpan.FromSeconds(3),
});

// To use a custom function to generate the delay for retries
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = 2,
    DelayGenerator = args =>
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
});

// To extract the delay from the result object
new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
{
    DelayGenerator = args =>
    {
        if (args.Outcome.Result is HttpResponseMessage responseMessage &&
            TryGetDelay(responseMessage, out TimeSpan delay))
        {
            return new ValueTask<TimeSpan?>(delay);
        }

        // Returning null means the retry strategy will use its internal delay for this attempt.
        return new ValueTask<TimeSpan?>((TimeSpan?)null);
    }
});

// To get notifications when a retry is performed
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = 2,
    OnRetry = args =>
    {
        Console.WriteLine("OnRetry, Attempt: {0}", args.AttemptNumber);

        // Event handlers can be asynchronous; here, we return an empty ValueTask.
        return default;
    }
});

// To keep retrying indefinitely or until success use int.MaxValue.
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = int.MaxValue,
});
```
<!-- endSnippet -->

If all retries fail, a retry strategy rethrows the final exception back to the calling code. For more details, visit the [retry strategy](https://www.pollydocs.org/strategies/retry) documentation.

### Circuit Breaker

<!-- snippet: circuit-breaker -->
```cs
// Add circuit breaker with default options.
// See https://www.pollydocs.org/strategies/circuit-breaker#defaults for defaults.
new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions());

// Add circuit breaker with customized options:
//
// The circuit will break if more than 50% of actions result in handled exceptions,
// within any 10-second sampling duration, and at least 8 actions are processed.
new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,
    SamplingDuration = TimeSpan.FromSeconds(10),
    MinimumThroughput = 8,
    BreakDuration = TimeSpan.FromSeconds(30),
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>()
});

// Handle specific failed results for HttpResponseMessage:
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<SomeExceptionType>()
            .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
    });

// Monitor the circuit state, useful for health reporting:
var stateProvider = new CircuitBreakerStateProvider();

new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddCircuitBreaker(new() { StateProvider = stateProvider })
    .Build();

/*
CircuitState.Closed - Normal operation; actions are executed.
CircuitState.Open - Circuit is open; actions are blocked.
CircuitState.HalfOpen - Recovery state after break duration expires; actions are permitted.
CircuitState.Isolated - Circuit is manually held open; actions are blocked.
*/

// Manually control the Circuit Breaker state:
var manualControl = new CircuitBreakerManualControl();

new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new() { ManualControl = manualControl })
    .Build();

// Manually isolate a circuit, e.g., to isolate a downstream service.
await manualControl.IsolateAsync();

// Manually close the circuit to allow actions to be executed again.
await manualControl.CloseAsync();
```
<!-- endSnippet -->

For more details, visit the [circuit breaker strategy](https://www.pollydocs.org/strategies/circuit-breaker) documentation.

### Fallback

<!-- snippet: fallback -->
```cs
// Add a fallback/substitute value if an operation fails.
new ResiliencePipelineBuilder<UserAvatar>()
    .AddFallback(new FallbackStrategyOptions<UserAvatar>
    {
        ShouldHandle = new PredicateBuilder<UserAvatar>()
            .Handle<SomeExceptionType>()
            .HandleResult(r => r is null),
        FallbackAction = args => Outcome.FromResultAsValueTask(UserAvatar.Blank)
    });

// Use a dynamically generated value if an operation fails.
new ResiliencePipelineBuilder<UserAvatar>()
    .AddFallback(new FallbackStrategyOptions<UserAvatar>
    {
        ShouldHandle = new PredicateBuilder<UserAvatar>()
            .Handle<SomeExceptionType>()
            .HandleResult(r => r is null),
        FallbackAction = args =>
        {
            var avatar = UserAvatar.GetRandomAvatar();
            return Outcome.FromResultAsValueTask(avatar);
        }
    });

// Use a default or dynamically generated value, and execute an additional action if the fallback is triggered.
new ResiliencePipelineBuilder<UserAvatar>()
    .AddFallback(new FallbackStrategyOptions<UserAvatar>
    {
        ShouldHandle = new PredicateBuilder<UserAvatar>()
            .Handle<SomeExceptionType>()
            .HandleResult(r => r is null),
        FallbackAction = args =>
        {
            var avatar = UserAvatar.GetRandomAvatar();
            return Outcome.FromResultAsValueTask(UserAvatar.Blank);
        },
        OnFallback = args =>
        {
            // Add extra logic to be executed when the fallback is triggered, such as logging.
            return default; // Returns an empty ValueTask
        }
    });
```
<!-- endSnippet -->

For more details, visit the [fallback strategy](https://www.pollydocs.org/strategies/fallback) documentation.

### Hedging

<!-- snippet: Hedging -->
```cs
// Add hedging with default options.
// See https://www.pollydocs.org/strategies/hedging#defaults for defaults.
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>());

// Add a customized hedging strategy that retries up to 3 times if the execution
// takes longer than 1 second or if it fails due to an exception or returns an HTTP 500 Internal Server Error.
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<SomeExceptionType>()
            .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError),
        MaxHedgedAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        ActionGenerator = args =>
        {
            Console.WriteLine("Preparing to execute hedged action.");

            // Return a delegate function to invoke the original action with the action context.
            // Optionally, you can also create a completely new action to be executed.
            return () => args.Callback(args.ActionContext);
        }
    });

// Subscribe to hedging events.
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
    {
        OnHedging = args =>
        {
            Console.WriteLine($"OnHedging: Attempt number {args.AttemptNumber}");
            return default;
        }
    });
```
<!-- endSnippet -->

If all hedged attempts fail, the hedging strategy will either re-throw the last exception or return the final failed result to the caller. For more details, visit the [hedging strategy](https://www.pollydocs.org/strategies/hedging) documentation.

### Timeout

The timeout resilience strategy assumes delegates you execute support [co-operative cancellation](https://learn.microsoft.com/dotnet/standard/threading/cancellation-in-managed-threads). You must use `Execute/Async(...)` overloads taking a `CancellationToken`, and the executed delegate must honor that `CancellationToken`.

<!-- snippet: timeout -->
```cs
// Add timeout using the default options.
// See https://www.pollydocs.org/strategies/timeout#defaults for defaults.
new ResiliencePipelineBuilder()
    .AddTimeout(new TimeoutStrategyOptions());

// To add a timeout with a custom TimeSpan duration
new ResiliencePipelineBuilder()
    .AddTimeout(TimeSpan.FromSeconds(3));

// To add a timeout using a custom timeout generator function
new ResiliencePipelineBuilder()
    .AddTimeout(new TimeoutStrategyOptions
    {
        TimeoutGenerator = args =>
        {
            // Note: the timeout generator supports asynchronous operations
            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
        }
    });

// To add a timeout and listen for timeout events
new ResiliencePipelineBuilder()
    .AddTimeout(new TimeoutStrategyOptions
    {
        TimeoutGenerator = args =>
        {
            // Note: the timeout generator supports asynchronous operations
            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
        },
        OnTimeout = args =>
        {
            Console.WriteLine($"{args.Context.OperationKey}: Execution timed out after {args.Timeout.TotalSeconds} seconds.");
            return default;
        }
    });
```
<!-- endSnippet -->

Timeout strategies throw `TimeoutRejectedException` when a timeout occurs. For more details, visit the [timeout strategy](https://www.pollydocs.org/strategies/timeout) documentation.

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
    .AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
    {
        PermitLimit = 100,
        Window = TimeSpan.FromMinutes(1)
    }));
```
<!-- endSnippet -->

Rate limiter strategy throws `RateLimiterRejectedException` if execution is rejected. For more details, visit the [rate limiter strategy](https://www.pollydocs.org/strategies/rate-limiter) documentation.

## Next steps

To learn more about Polly, visit [pollydocs.org][polly-docs].

## Samples

- [Samples](samples/README.md): Samples in this repository that serve as an introduction to Polly.
- [Polly-Samples](https://github.com/App-vNext/Polly-Samples): Contains practical examples for using various implementations of Polly. Please feel free to contribute to the Polly-Samples repository in order to assist others who are either learning Polly for the first time, or are seeking advanced examples and novel approaches provided by our generous community.
- Microsoft's [eShopOnContainers project](https://github.com/dotnet-architecture/eShopOnContainers): Sample project demonstrating a .NET Micro-services architecture and using Polly for resilience.

## License

Licensed under the terms of the [New BSD License](https://opensource.org/license/bsd-3-clause/)

[polly-docs]: https://www.pollydocs.org/
