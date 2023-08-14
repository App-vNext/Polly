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

    public ValueTask ExecuteValueTaskAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default);

    public ValueTask<TResult> ExecuteValueTaskAsync(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default);
    
    // omitted for simplicity
}
```

The [ResilienceContext](ResilienceContext.cs) is defined as:

``` csharp
public sealed class ResilienceContext
{
    public CancellationToken CancellationToken { get; set; }

    public bool IsSynchronous { get; }

    public bool IsVoid { get; }

    public bool ContinueOnCapturedContext { get; }

    public Type ResultType { get; }

    // omitted for simplicity
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

    context.IsSynchronous = true;
    context.ResultType = typeof(VoidResult);

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

For example we have non-reactive delay strategy that decides how to execute this user-callback by reading the `ResilienceContext`:

``` csharp
internal class DelayStrategy : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;

    public DelayStrategy(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    protected override async ValueTask<T> ExecuteCore<T, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, 
        ResilienceContext context, 
        TState state)
    {
        await _timeProvider.DelayAsync(context).ContinueOnCapturedContext(context.ContinueOnCapturedContext);

        return await callback(context, state).ContinueOnCapturedContext(context.ContinueOnCapturedContext);
    }
}
```

In the preceding example we are calling the `DelayAsync` extension for `TimeProvider` that accepts the `ResilienceContext`. The extension is using `Thread.Sleep` for synchronous executions and `Task.Delay` for asynchronous executions.

This way, the responsibility of how to execute method is lifted from the user and instead passed to the policy. User cares only about the `ResiliencePipeline` class. User uses only a single strategy to execute all scenarios. Previously, user had to decide whether to use sync vs async, typed vs non-typed policies.

The life of extensibility author is also simplified as they only maintain one implementation of strategy instead of multiple ones. See the duplications in [`Polly.Retry`](https://github.com/App-vNext/Polly/tree/main/src/Polly/Retry).

### About Synchronous and Asynchronous Executions

Polly's core, from version 8, fundamentally focuses on asynchronous executions. However, it also supports synchronous executions, which require minimal effort for authors developing custom resilience strategies. This support is enabled by passing and wrapping the synchronous callback provided by the user into an asynchronous one, which returns a completed `ValueTask` upon completion. This feature allows custom resilience strategies to treat all executions as asynchronous. In cases of synchronous execution, the method simply returns a completed task upon awaiting.

There are scenarios where the resilience strategy necessitates genuine asynchronous work. In such cases, authors might decide to optimize for synchronous executions. For instance, they may use `Thread.Sleep` instead of `Task.Delay`. To facilitate this, Polly exposes the `ResilienceContext.IsSynchronous` property, which authors can leverage. It's worth noting, though, that optimizing for synchronous executions might add significant complexity for the author. As a result, some authors may opt to execute the code asynchronously.

A common scenario that illustrates this is the circuit breaker, which allows for hundreds of concurrent executions. In failure scenarios, only one will trigger the opening of the circuit. If this single execution was synchronous, it would involve some synchronous-over-asynchronous code. This situation occurs because, in the circuit breaker, we wanted to avoid duplicating code for synchronous executions. However, this does not impact the scalability of the Circuit Breaker, since such events are rare and do not execute on the hot path.

### Generic Resilience Strategy

Polly also exposes the `ResiliencePipeline<T>` that is just a simple wrapper over `ResiliencePipeline`. This pipeline is used for scenarios when the consumer handles the single result type.

## Creation of `ResiliencePipeline`

This API exposes the following builders:

- [ResiliencePipelineBuilder](ResiliencePipelineBuilder.cs): Used to create resilience strategies that can execute all types of callbacks. In general, these strategies only handle exceptions. 
- [ResiliencePipelineBuilder<T>](ResiliencePipelineBuilder.TResult.cs): Used to create generic resilience strategies that can only execute callbacks that return the same result type.
- [ResiliencePipelineBuilderBase](ResiliencePipelineBuilderBase.cs): The base class for both builders above. You can use it as a target for strategy extensions that work for both builders above.  

To create a strategy or composite resilience strategy you chain various extensions for `ResiliencePipelineBuilder` followed by the `Build` call:

Pipeline with a single strategy:

``` csharp
var ResiliencePipeline = new ResiliencePipelineBuilder().AddRetry(new()).Build();
```

Pipeline wiht multiple strategies:

``` csharp
var ResiliencePipeline = new ResiliencePipelineBuilder()
    .AddRetry(new())
    .AddCircuitBreaker(new())
    .AddTimeout(new TimeoutStrategyOptions() { ... })
    .Build();
```

## Extensibility

The resilience extensibility is simple. You just expose extensions for `ResiliencePipelineBuilder` that use the `ResiliencePipelineBuilder.AddStrategy` methods.

If you want to create a resilience strategy that works for both generic and non-generic builders you can use `ResiliencePipelineBuilderBase` as a target:

``` csharp
public static TBuilder AddMyStrategy<TBuilder>(this TBuilder builder)
    where TBuilder : ResiliencePipelineBuilderBase
{
    return builder.AddStrategy(new MyStrategy());
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
- `Func<OutcomeArguments<T, TArgs>, ValueTask<bool>>`: This is the predicate for the generic outcome.
- `Func<OutcomeArguments<object, TArgs>, ValueTask<bool>>`: This is the predicate for the non-generic outcome.

**Events**
- `Func<OutcomeArguments<T, TArgs>, ValueTask>`: This is the event for the generic outcome.
- `Func<OutcomeArguments<object, TArgs>, ValueTask>`: This is the event for the non-generic outcome.
- `Func<Args, ValueTask>`: This is the event utilized by strategies that do not operate with an outcome (for example, Timeout, RateLimiter).

**Generators**
- `Func<OutcomeArguments<T, TArgs>, ValueTask<TValue>>`: This is the generator for the generic outcome.
- `Func<OutcomeArguments<object, TArgs>, ValueTask<TValue>>`: This is the generator for the non-generic outcome.
- `Func<Args, ValueTask<TValue>>`: This is the generator used by strategies that do not operate with an outcome (for example, Timeout, RateLimiter).

It's essential to note that all these delegates are asynchronous and return a `ValueTask`. 

The **`OutcomeArguments<T, TArgs>`** captures the following information that can be used by the delegate:

- `Outcome<T>`: This captures the result of an operation that yields a result of a specific type, `TResult`, or an exception.
- `Context`: The `ResilienceContext` of the operation.
- `Arguments`: Additional arguments associated with the operation. Each resilience strategy can define different arguments for different operations or events.

## Examples

Below are a few examples showcasing the usage of these delegates:

A non-generic predicate defining retries for multiple result types:

``` csharp
new ResiliencePipelineBuilder()
   .AddRetry(new RetryStrategyOptions
    {
        ShouldRetry = args => args switch
        {
            { Exception: InvalidOperationException } => PredicateResult.True,
            { Result: string result } when result == Failure => PredicateResult.True,
            { Result: int result } when result == -1 => PredicateResult.True,
            _ => PredicateResult.False
        },
    })
    .Build();
```

A generic predicate defining retries for a single result type:

``` csharp
new ResiliencePipelineBuilder()
   .AddRetry(new RetryStrategyOptions<string>
    {
        ShouldRetry = args => args switch
        {
            { Exception: InvalidOperationException } => PredicateResult.True,
            { Result: result } when result == Failure => PredicateResult.True,
            _ => PredicateResult.False
        },
    })
    .Build();
```

## Registering Custom Callbacks

When setting the delegates, ensure to respect the `ResilienceContext.IsSynchronous` property's value and execute your delegates synchronously for synchronous executions. In addition, use the `ResilienceContext.ContinueOnCapturedContext` property when your user code uses execution with synchronization context (for example, asynchronous calls in UI applications, such as in Windows Forms or WPF applications).

## Telemetry

Each individual resilience strategy can emit telemetry by using the [`ResiliencePipelineTelemetry`](Telemetry/ResiliencePipelineTelemetry.cs) API. Polly wraps the arguments as [`TelemetryEventArguments`](Telemetry/TelemetryEventArguments.cs) and emits them using `DiagnosticSource`.
To consume the telemetry, Polly adopters needs to assign an instance of `DiagnosticSource` to `ResiliencePipelineBuilder.DiagnosticSource` and consume `TelemetryEventArguments`.

For common use-cases, it is anticipated that Polly users would leverage `Polly.Extensions`. This allows all of the aforementioned functionalities by invoking the `ResiliencePipelineBuilder.ConfigureTelemetry(...)` extension method. `ConfigureTelemetry` processes `TelemetryEventArguments` and generates logs and metrics from it.
