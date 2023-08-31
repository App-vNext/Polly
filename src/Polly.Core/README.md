# Introduction

The Polly V8 API exposes unified and non-allocating resilience API that is described in the sections below.

## Core API

At the heart of Polly V8 is the [ResiliencePipeline](ResiliencePipeline.cs) class that is responsible for execution of user code. It's one class that handles all Polly V7 scenarios:

- `ISyncPolicy`
- `IAsyncPolicy`
- `ISyncPolicy<T>`
- `IAsyncPolicy<T>`

``` csharp
public abstract class ResiliencePipeline
{
    public void Execute(Action callback);

    public TResult Execute<TResult>(Func<TResult> callback);

    public Task ExecuteAsync(Func<CancellationToken, Task> callback, CancellationToken cancellationToken = default);

    public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> callback, CancellationToken cancellationToken = default);

    public ValueTask ExecuteAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default);

    public ValueTask<TResult> ExecuteAsync(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default);
    
    // omitted for simplicity
}
```

The [ResilienceContext](ResilienceContext.cs) is defined as:

``` csharp
public sealed class ResilienceContext
{
    public string? OperationKey { get; }

    public CancellationToken CancellationToken { get; }

    public bool ContinueOnCapturedContext { get; }

    public ResilienceProperties Properties { get; }
}
```

The `ResiliencePipeline` unifies the 4 different policies used now in Polly. User actions are executed under a single API. The are many methods
exposed on this class that cover different scenarios:

- Synchronous void methods.
- Synchronous methods with result.
- Asynchronous void methods.
- Asynchronous methods with result.

For example, the synchronous `Execute` method is implemented as:

``` csharp
public void Execute(Action execute)
{
    var context = ResilienceContextPool.Shared.Get();

    context.IsSynchronous = true; // internal to Polly
    context.ResultType = typeof(VoidResult); // internal to Polly

    try
    {
        ExecuteCore(static (context, state) =>
        {
            state();
            return new ValueTask<Outcome<VoidResult>>(new(VoidResult.Instance));
        }, 
        context, 
        execute).GetAwaiter().GetResult();
    }
    finally
    {
        ResilienceContextPool.Shared.Return(context);
    }
}
```

In the preceding example:

- We rent a `ResilienceContext` from the pool.
- We store the information about the execution mode by setting the `IsSynchronous` and `ResultType` properties on the context. Here, we use internal `VoidResult` marker to say this user-callback returns no result.
- We pass the user-callback, and use the `State` to avoid closure allocation.
- We block the execution.
- We return `ResilienceContext` to the pool.

The resilience pipeline is composed of a single or multiple individual resilience strategies. Polly V8 recognizes the following building blocks for resilience strategies:

- `ResilienceStrategy`: Base class for all non-reactive resilience strategies.
- `ResilienceStrategy<T>`: Base class for all reactive resilience strategies.

As an example, we have non-reactive strategy that executes the user-provided callback:

``` csharp
internal class MyCustomStrategy : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;

    public LoggingStrategy(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    protected override async ValueTask<T> ExecuteCore<T, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, 
        ResilienceContext context, 
        TState state)
    {
        // do something before execution

        var outcome = await callback(context, state).ContinueOnCapturedContext(context.ContinueOnCapturedContext);

        // do something after execution

        return outcome;
    }
}
```

### About Synchronous and Asynchronous Executions

Polly's core, from version 8, fundamentally focuses on asynchronous executions. However, it also supports synchronous executions, which require minimal effort for authors developing custom resilience strategies. This support is enabled by passing and wrapping the synchronous callback provided by the user into an asynchronous one, which returns a completed `ValueTask` upon completion. This feature allows custom resilience strategies to treat all executions as asynchronous. In cases of synchronous execution, the method simply returns a completed task upon awaiting.

### Generic Resilience Strategy

Polly also exposes the `ResiliencePipeline<T>` that is just a simple wrapper over `ResiliencePipeline`. This pipeline is used for scenarios when the consumer handles only a single result type.

## Creation of `ResiliencePipeline`

This API exposes the following builders:

- [ResiliencePipelineBuilder](ResiliencePipelineBuilder.cs): Used to create resilience strategies that can execute all types of callbacks. In general, these strategies only handle exceptions. 
- [ResiliencePipelineBuilder<T>](ResiliencePipelineBuilder.TResult.cs): Used to create generic resilience strategies that can only execute callbacks that return the same result type.
- [ResiliencePipelineBuilderBase](ResiliencePipelineBuilderBase.cs): The base class for both builders above. You can use it as a target for strategy extensions that work for both builders above.  

To create a resilience pipeline you chain various extensions for `ResiliencePipelineBuilder` followed by the `Build` call:

Pipeline with a single strategy:

``` csharp
var ResiliencePipeline = new ResiliencePipelineBuilder().AddRetry(new()).Build();
```

Pipeline with multiple strategies:

``` csharp
var ResiliencePipeline = new ResiliencePipelineBuilder()
    .AddRetry(new())
    .AddCircuitBreaker(new())
    .AddTimeout(new TimeoutStrategyOptions() { ... })
    .Build();
```

## Extensibility

The resilience extensibility is simple. You just expose extensions for `ResiliencePipelineBuilder` that use the `AddStrategy` extensions methods.

If you want to create a resilience strategy that works for both generic and non-generic builders you can use `ResiliencePipelineBuilderBase` as a target:

``` csharp
public static TBuilder AddMyCustomStrategy<TBuilder>(this TBuilder builder, MyCustomStrategyOptions options)
    where TBuilder : ResiliencePipelineBuilderBase
{
    return builder.AddStrategy(context => new MyCustomStrategy(), options);
}
```

# Resilience Strategy Delegates

Individual resilience strategies leverage the following delegate types:

- **Predicates**: These are essential when a resilience strategy needs to determine whether or not to handle the execution result.
- **Events**: These are invoked when significant events occur within the resilience strategy.
- **Generators**: These are used when the resilience strategy requires a certain value from the caller.

## Delegate Signature Guidelines

The suggested signatures for these delegates are as follows:

**Predicates**
- `Func<Args<TResult>, ValueTask<bool>>` (Reactive)

**Events**
- `Func<Args<TResult>, ValueTask>` (Reactive)
- `Func<Args, ValueTask>` (Non-Reactive)

**Generators**
- `Func<Args<TResult>, ValueTask<TValue>>` (Reactive)
- `Func<Args, ValueTask<TValue>>` (Non-Reactive)

Notice that the delegates accept the `Args` and `Args<TResult>` argument. These arguments represent the information about the event. It's essential to note that all these delegates are asynchronous and return a `ValueTask`. 

For non-reactive strategies the `Args` could look like:

``` csharp
public readonly struct OnTimeoutArguments
{
    public OnTimeoutArguments(ResilienceContext context, TimeSpan timeout)
    {
        Context = context;
        Timeout = timeout;
    }

    public ResilienceContext Context { get; } // you should always include the Context property 

    public TimeSpan Timeout { get; } // additional properties related to the event
}
```

For reactive strategies the `Args<TResult>` could look like:

``` csharp
public readonly struct OnRetryArguments<TResult>
{
    public OnRetryArguments(ResilienceContext context, Outcome<TResult> outcome, int attemptNumber)
    {
        Context = context;
        Outcome = outcome;
        AttemptNumber = attemptNumber;
    }

    public ResilienceContext Context { get; } // you should always include the Context property

    public Outcome<TResult> Outcome { get; } // include the outcome associated with the event

    public int AttemptNumber { get; }
}
```

## Examples

Below are a few examples showcasing the usage of these delegates:

A non-generic predicate defining retries for multiple result types:

``` csharp
new ResiliencePipelineBuilder()
   .AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = args => args switch
        {
            { Exception: InvalidOperationException } => PredicateResult.True(),
            { Result: string result } when result == Failure => PredicateResult.True(),
            { Result: int result } when result == -1 => PredicateResult.True(),
            _ => PredicateResult.False()
        },
    })
    .Build();
```

A generic predicate defining retries for a single result type:

``` csharp
new ResiliencePipelineBuilder()
   .AddRetry(new RetryStrategyOptions<string>
    {
        ShouldHandle = args => args switch
        {
            { Exception: InvalidOperationException } => PredicateResult.True(),
            { Result: result } when result == Failure => PredicateResult.True(),
            _ => PredicateResult.False()
        },
    })
    .Build();
```

## Registering Custom Callbacks

When setting the delegates, use the `ResilienceContext.ContinueOnCapturedContext` property when your user code uses execution with synchronization context (for example, asynchronous calls in UI applications, such as in Windows Forms or WPF applications).

## Telemetry

Each individual resilience strategy can emit telemetry by using the [`ResiliencePipelineTelemetry`](Telemetry/ResiliencePipelineTelemetry.cs) API. Polly wraps the arguments as [`TelemetryEventArguments`](Telemetry/TelemetryEventArguments.cs) and emits them using `TelemetryListener`.
To consume the telemetry, Polly adopters needs to assign an instance of `TelemetryListener` to `ResiliencePipelineBuilder.TelemetryListener` and consume `TelemetryEventArguments`.

For common use-cases, it is anticipated that Polly users would leverage `Polly.Extensions`. This allows all of the aforementioned functionalities by invoking the `ResiliencePipelineBuilder.ConfigureTelemetry(...)` extension method. `ConfigureTelemetry` processes `TelemetryEventArguments` and generates logs and metrics from it.
