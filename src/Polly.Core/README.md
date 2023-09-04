# Polly V8 API Documentation

The Polly V8 API offers a unified, non-allocating resilience API, detailed in the sections below.

## Introduction

At the core of Polly V8 is the [`ResiliencePipeline`](ResiliencePipeline.cs) class, responsible for executing user-provided callbacks. This class handles all scenarios covered in Polly V7, such as:

- `ISyncPolicy`
- `IAsyncPolicy`
- `ISyncPolicy<T>`
- `IAsyncPolicy<T>`

```csharp
public abstract class ResiliencePipeline
{
    public void Execute(Action callback);

    public TResult Execute<TResult>(Func<TResult> callback);
    
    public Task ExecuteAsync(
        Func<CancellationToken, Task> callback, 
        CancellationToken cancellationToken = default);
    
    public Task<TResult> ExecuteAsync(
        Func<CancellationToken, Task<TResult>> callback, 
        CancellationToken cancellationToken = default);
    
    public ValueTask ExecuteAsync(
        Func<CancellationToken, ValueTask> callback, 
        CancellationToken cancellationToken = default);
    
    public ValueTask<TResult> ExecuteAsync(
        Func<CancellationToken, ValueTask<TResult>> callback, 
        CancellationToken cancellationToken = default);
    
    // Other methods are omitted for simplicity
}
```

The [`ResilienceContext`](ResilienceContext.cs) is defined as follows:

```csharp
public sealed class ResilienceContext
{
    public string? OperationKey { get; }
    public CancellationToken CancellationToken { get; }
    public bool ContinueOnCapturedContext { get; }
    public ResilienceProperties Properties { get; }
}
```

The `ResiliencePipeline` class unifies the four different policies that were available in Polly v7, enabling user actions to be executed via a single API. This class offers various methods to handle different scenarios:

- Synchronous methods without a return value.
- Synchronous methods that return a value.
- Asynchronous methods without a return value.
- Asynchronous methods that return a value.

> [!NOTE]
> Polly also provides a `ResiliencePipeline<T>` class. This specialized pipeline is useful for scenarios where the consumer is concerned with only a single type of result.

## Resilience Strategies

The resilience pipeline may consist of one or more individual resilience strategies. Polly V8 categorizes resilience strategies into the following building blocks:

- `ResilienceStrategy`: Base class for all non-reactive resilience strategies.
- `ResilienceStrategy<T>`: Base class for all reactive resilience strategies.

### Example: Custom Non-Reactive Strategy

Here's an example of a non-reactive strategy that executes a user-provided callback:

<!-- snippet: my-custom-strategy -->
```cs
internal class MyCustomStrategy : ResilienceStrategy
{
    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        // Perform actions before execution

        var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        // Perform actions after execution

        return outcome;
    }
}
```
<!-- endSnippet -->

### About Synchronous and Asynchronous Executions

Polly's core, from version 8, fundamentally focuses on asynchronous executions. However, it also supports synchronous executions, which require minimal effort for authors developing custom resilience strategies. This support is enabled by passing and wrapping the synchronous callback provided by the user into an asynchronous one, which returns a completed `ValueTask` upon completion. This feature allows custom resilience strategies to treat all executions as asynchronous. In cases of synchronous execution, the method simply returns a completed task upon awaiting.

## Creating a `ResiliencePipeline`

The API exposes the following builder classes for creating resilience pipelines:

- [`ResiliencePipelineBuilder`](ResiliencePipelineBuilder.cs): Useful for creating resilience strategies capable of executing any type of callback. Typically, these strategies are focused on exception handling.
- [`ResiliencePipelineBuilder<T>`](ResiliencePipelineBuilder.TResult.cs): Aimed at creating generic resilience strategies that execute callbacks returning a specific result type.
- [`ResiliencePipelineBuilderBase`](ResiliencePipelineBuilderBase.cs): This serves as the base class for both of the builders mentioned above. It can be used as a target for strategy extensions compatible with either of the two.

To construct a resilience pipeline, chain various extensions on the `ResiliencePipelineBuilder` and conclude with a `Build` method call.

### Creating a non-generic pipeline

<!-- snippet: create-generic-pipeline -->
```cs
ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>()
    .AddRetry(new())
    .AddCircuitBreaker(new())
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();
```
<!-- endSnippet -->

### Creating a generic pipeline

<!-- snippet: create-non-generic-pipeline -->
```cs
ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>()
    .AddRetry(new())
    .AddCircuitBreaker(new())
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();
```
<!-- endSnippet -->

## Extensibility

Extending the resilience functionality is straightforward. You can create extensions for `ResiliencePipelineBuilder` by leveraging the `AddStrategy` extension methods. If you aim to design a resilience strategy that is compatible with both generic and non-generic builders, consider using `ResiliencePipelineBuilderBase` as your target class.

Here's an example:

<!-- snippet: add-my-custom-strategy -->
```cs
public static TBuilder AddMyCustomStrategy<TBuilder>(this TBuilder builder, MyCustomStrategyOptions options)
    where TBuilder : ResiliencePipelineBuilderBase
{
    return builder.AddStrategy(context => new MyCustomStrategy(), options);
}

public class MyCustomStrategyOptions : ResilienceStrategyOptions
{
    public MyCustomStrategyOptions()
    {
        Name = "MyCustomStrategy";
    }
}
```
<!-- endSnippet -->

To gain insights into implementing custom resilience strategies, you can explore the following Polly strategy examples:

- [**Retry**](Retry/): Demonstrates how to implement a reactive resilience strategy.
- [**Timeout**](Timeout/): Provides guidance on implementing a non-reactive resilience strategy.
- [**Extensibility Sample**](../../samples/Extensibility/): Offers a practical example of creating a custom resilience strategy.

## Resilience Strategy Delegates

Individual resilience strategies make use of several delegate types:

- **Predicates**: Vital for determining whether a resilience strategy should handle the given execution result.
- **Events**: Triggered when significant actions or states occur within the resilience strategy.
- **Generators**: Invoked when the resilience strategy needs specific information or values from the caller.

Recommended signatures for these delegates are:

**Predicates**

- `Func<Args<TResult>, ValueTask<bool>>` (Reactive)

**Events**

- `Func<Args<TResult>, ValueTask>` (Reactive)
- `Func<Args, ValueTask>` (Non-Reactive)

**Generators**

- `Func<Args<TResult>, ValueTask<TValue>>` (Reactive)
- `Func<Args, ValueTask<TValue>>` (Non-Reactive)


These delegates accept either `Args` or `Args<TResult>` arguments, which encapsulate event information. Note that all these delegates are asynchronous and return a `ValueTask`.

> [!NOTE]
> When setting up delegates, consider using the `ResilienceContext.ContinueOnCapturedContext` property if your user code interacts with a synchronization context (as in asynchronous UI applications like Windows Forms or WPF).

### Delegate Arguments

For non-reactive strategies, the `Args` structure might resemble:

<!-- snippet: on-timeout-args -->
```cs
public readonly struct OnTimeoutArguments
{
    public OnTimeoutArguments(ResilienceContext context, TimeSpan timeout)
    {
        Context = context;
        Timeout = timeout;
    }

    public ResilienceContext Context { get; } // Include the Context property

    public TimeSpan Timeout { get; } // Additional event-related properties
}
```
<!-- endSnippet -->

### Example: Usage of Delegates

Below are some examples illustrating the usage of these delegates:

<!-- snippet: delegate-usage -->
```cs
new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        // Non-Generic predicate for multiple result types
        ShouldHandle = args => args.Outcome switch
        {
            { Exception: InvalidOperationException } => PredicateResult.True(),
            { Result: string result } when result == "Failure" => PredicateResult.True(),
            { Result: int result } when result == -1 => PredicateResult.True(),
            _ => PredicateResult.False()
        },
    })
    .Build();

new ResiliencePipelineBuilder<string>()
    .AddRetry(new RetryStrategyOptions<string>
    {
        // Generic predicate for a single result type
        ShouldHandle = args => args.Outcome switch
        {
            { Exception: InvalidOperationException } => PredicateResult.True(),
            { Result: { } result } when result == "Failure" => PredicateResult.True(),
            _ => PredicateResult.False()
        },
    })
    .Build();
```
<!-- endSnippet -->

## Telemetry

Each resilience strategy can generate telemetry data through the [`ResiliencePipelineTelemetry`](Telemetry/ResiliencePipelineTelemetry.cs) API. Polly encapsulates event details as [`TelemetryEventArguments`](Telemetry/TelemetryEventArguments.cs) and emits them via `TelemetryListener`.

To leverage this telemetry data, users should assign a `TelemetryListener` instance to `ResiliencePipelineBuilder.TelemetryListener` and then consume the `TelemetryEventArguments`.

For common scenarios, it is expected that users would make use of `Polly.Extensions`. This extension enables telemetry configuration through the `ResiliencePipelineBuilder.ConfigureTelemetry(...)` method, which processes `TelemetryEventArguments` to generate logs and metrics.
