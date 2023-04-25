# Introduction

The Polly V8 API exposes unified and non-allocating resilience API that is described in the sections below.

## Core API

At the heart of Polly V8 is the [ResilienceStrategy](ResilienceStrategy.cs) class that is responsible for execution of user code. It's one class that handles all Polly V7 scenarios:

- `ISyncPolicy`
- `IAsyncPolicy`
- `ISyncPolicy<T>`
- `IAsyncPolicy<T>`

``` csharp
public abstract class ResilienceStrategy
{
    // the main method that all the others call
    protected virtual ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> execution, ResilienceContext context, TState state);

    // convenience methods for various types of user-callbacks
    public void Execute(Action callback);

    public TResult Execute<TResult>(Func<TResult> callback);

    public Task ExecuteTaskAsync(Func<CancellationToken, Task> callback, CancellationToken cancellationToken = default);

    public Task<TResult> ExecuteTaskAsync(Func<CancellationToken, Task<TResult>> callback, CancellationToken cancellationToken = default);

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

The `ResilienceStrategy` unifies the 4 different policies used now in Polly. User actions are executed under a single API. The are many methods
exposed on this class that cover different scenarios:

- Synchronous void methods.
- Synchronous methods with result.
- Asynchronous void methods.
- Asynchronous methods with result.

For example, the synchronous `Execute` method is implemented as:

``` csharp
public void Execute(Action execute)
{
    var context = ResilienceContext.Get();

    context.IsSynchronous = true;
    context.ResultType = typeof(VoidResult);

    try
    {
        strategy.ExecuteCoreAsync(static (context, state) =>
        {
            state();
            return new ValueTask<VoidResult>(VoidResult.Instance);
        }, 
        context, 
        execute).GetAwaiter().GetResult();
    }
    finally
    {
        ResilienceContext.Return(context);
    }
}
```

In the preceding example:

- We rent a `ResilienceContext` from the pool.
- We store the information about the execution mode by setting the `IsSynchronous` and `ResultType` properties on the context. Here, we use internal `VoidResult` marker to say this user-callback returns no result.
- We pass the user-callback, and use the `State` to avoid closure allocation.
- We block the execution.
- We return `ResilienceContext` to the pool.

Underlying implementation decides how to execute this user-callback by reading the `ResilienceContext`:

``` csharp
internal class DelayStrategy : ResilienceStrategy
{
    protected override async ValueTask<T> ExecuteCoreAsync<T, TState>(Func<ResilienceContext, TState, ValueTask<T>> callback, ResilienceContext context, TState state)
    {
        if (context.IsSynchronous)
        {
            Thread.Sleep(1000);
        }
        else
        {
            await Task.Delay(1000).ContinueOnCapturedContext(context.ContinueOnCapturedContext);
        }

        return await callback(context, state).ContinueOnCapturedContext(context.ContinueOnCapturedContext);
    }
}
```

In the preceding example:

- For synchronous execution we are using `Thread.Sleep`.
- For asynchronous execution we are using `Task.Delay`.

This way, the responsibility of how to execute method is lifted from the user and instead passed to the policy. User cares only about the `ResilienceStrategy` class. User uses only a single strategy to execute all scenarios. Previously, user had to decide whether to use sync vs async, typed vs non-typed policies.

The life of extensibility author is also simplified as they only maintain one implementation of strategy instead of multiple ones. See the duplications in [`Polly.Retry`](https://github.com/App-vNext/Polly/tree/main/src/Polly/Retry).

## Creation of `ResilienceStrategy`

This API exposes [ResilienceStrategyBuilder](Builder/ResilienceStrategyBuilder.cs) that can be used to create the resilience strategy:

``` csharp
public interface ResilienceStrategyBuilder
{
    ResilienceStrategyBuilderOptions Options { get; set; }

    ResilienceStrategyBuilder AddStrategy(ResilienceStrategy strategy, ResilienceStrategyOptions? options = null);

    ResilienceStrategyBuilder AddStrategy(Func<ResilienceStrategyBuilderContext, ResilienceStrategy> factory, ResilienceStrategyOptions? options = null);

    ResilienceStrategy Build();
}
```

To create a strategy or pipeline of strategies you chain various extensions for `ResilienceStrategyBuilder` followed by the `Build` call:

Single strategy:

``` csharp
var resilienceStrategy = new ResilienceStrategyBuilder().AddRetry().Build();
```

Pipeline of strategies:

``` csharp
var resilienceStrategy = new ResilienceStrategyBuilder()
    .AddRetry()
    .AddCircuitBreaker()
    .AddTimeout(new TimeoutStrategyOptions() { ... })
    .Build();
```

## Extensibility

The resilience extensibility is simple. You just expose extensions for `ResilienceStrategyBuilder` that use the `ResilienceStrategyBuilder.AddStrategy` methods.

## Callback API

The callback can be used by individual resilience strategies to simplify the handling of various result types. It's use is entirely optional.  It's recommended to all authors of custom resilience strategies. The core of the callback API is includes components, such as the `IResilienceArguments` interface, the `Outcome` struct, and the `Outcome<TResult>` struct.

These core components are then used in the following callback types (explained in each respective section):

- **Generators**
- **Events**
- **Predicates**

All the callback types above use the following components:

- **`IResilienceArguments`**: Defines the recommended structure for arguments utilized by individual strategies. It exposes a single property, `Context`, which provides the context associated with the execution of a user-provided callback.
- **`Outcome<TResult>`**: Captures the outcome of an operation that yields a result of a specific type, `TResult`, or an exception. This struct specializes its functionality to accommodate generic results. The `TryGetResult` method, for instance, is tailored to handle the generic result type, offering additional flexibility and extensibility.
- **`Outcome`**: represents a non-generic outcome of an operation, encompassing both a result and an exception, if any. This struct is equipped with several properties and methods, such as `HasResult`, `IsVoidResult`, and `TryGetResult`, which facilitate the handling and evaluation of the outcome of an operation.

### Events

Events are designed to handle various scenarios, such as events without outcomes, events with specific result types, and void-based events. They allow registering callbacks with different signatures, making it convenient to add synchronous and asynchronous event handlers.

- **`NoOutcomeEvent<TArgs>`**: holds a list of callbacks that are invoked when some event occurs. These callbacks are executed for all result types and do not require any `Outcome`. This class supports registering multiple event callbacks. The registered callbacks are executed one-by-one in the same order as they were registered.
- **`OutcomeEvent<TArgs>`**: is designed for events that use `Outcome<TResult>` and `TArgs` in the registered event callbacks. This class allows registering callbacks for specific result types or for all result types, including void-based results.
- `OutcomeEvent<TArgs, TResult>`: class is  base class for events that use `Outcome<TResult>` and `TArgs` in the registered event callbacks. This class allows registering callbacks for a specific result type.
- **`VoidOutcomeEvent<TArgs>`**: class is designed for events that use `Outcome` and `TArgs` in the registered event callbacks specifically for void-based results. This class allows registering callbacks for void-based results.

API Usage:

``` csharp
var options = new RetryStrategyOptions();
OutcomeEvent<OnRetryArguments> retryEvent = options.OnRetry;

retryEvent
    .Register(() => { }) // called for all result types
    .Register<int>(() => { }) // called for only int result types
    .RegisterVoid(() => { }) // called for void-based result types
    .Register(async (outcome, args) => await OnEventAsync(outcome, args)) // called asynchronously with the outcome and arguments for all result types
```

The code above demonstrates how `OutcomeEvent<OnRetryArguments>` can be used to register multiple synchronous and asynchronous callbacks with different signature under a single unified API.

### Predicates

Predicates are designed to provide the resilience strategy that uses them with the information about whether the specific outcome should or shouldn't be handled by the strategy. The predicate API allows registering many predicates with different signatures. These can be:

- Asynchronous predicates
- Synchronous predicates
- Exception predicates
- Combinations of types above that take arguments or outcome in the predicate arguments

The API exposes the following built-in  predicates:

- **`OutcomePredicate<TArgs>`**: holds a list of predicates for various result types. The first predicate that returns `true` wins, if no predicate returns true the result is a `false` value indicating that the result should not be handled.
- **`OutcomePredicate<TArgs, TResult>`**: holds a list of predicates for a single result type. The first predicate that returns `true` wins, if no predicate returns true the result is a `false` value indicating that the result should not be handled.
- **`VoidOutcomePredicate<TArgs>`**: holds a list of predicates for a void result type. The first predicate that returns `true` wins, if no predicate returns true the result is a `false` value indicating that the result should not be handled.

API Usage:

``` csharp
var options = new RetryStrategyOptions();
OutcomePredicate<ShouldRetryArguments> predicate = options.ShouldRetry;

predicate
    .HandleException<InvalidOperationException>() // handle exception
    .HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode) // access to response message
    .HandleResult<double>(result => result > 0.5) // access to result
    .HandleOutcome<int>((outcome, args) => { }) // access to both outcome and arguments
    .HandleOutcome<int>((outcome, args) => await ShouldRetryAsync(outcome, args)) // access to both outcome and arguments
```

The code above demonstrates how `OutcomePredicate<ShouldRetryArguments>` can be used to register multiple synchronous and asynchronous fallbacks with different signature under a single unified API.

### Generators

Generates are designed to generate a value of specific type for a resilience strategy that uses them. We recognize the following generator types:

- **`NoOutcomeGenerator<TArgs>`**: the generators that does not require any `Outcome` to produce a value. Used in resilience strategies that do not access the `Outcome` such as timeout strategy.
- **`OutcomeGenerator<TArgs, TValue>`**: holds a list of generators for various result types. These generators produce the value of `TValue` type.
- **`OutcomeGenerator<TArgs, TValue, TResult>`**: holds a generator for a specific result type.
- **`VoidOutcomeGenerator<TArgs, TValue>`**: holds a generator for a void-based result type.

API Usage:

``` csharp
var options = new RetryStrategyOptions();
OutcomeGenerator<RetryDelayArguments, TimeSpan> generator = options.RetryDelayGenerator;

generator
    .SetGenerator((outcome, args) => TimeSpan.FromSeconds(args.Attempt)) // called for all result types
    .SetGenerator<int>((outcome, args) => TimeSpan.FromSeconds(args.Attempt)) // called for int result type only
    .SetVoidGenerator((outcome, args) => TimeSpan.FromSeconds(args.Attempt)) // called for void result type only
    .SetVoidGenerator(async (outcome, args) => await GenerateDelayAsync(out, args));
```

The code above demonstrates how `OutcomeGenerator<RetryDelayArguments, TimeSpan>` can be used to register multiple synchronous and asynchronous generators with different signature under a single unified API.

### Performance

The callback API is non-allocating and fast. However, the performance varies depending on number of callbacks you register. Maximum performance is archived when your callbacks handles only a single result type with a single registered callback. In that scenario the underlying handler does not do any dictionary lookups.

### Registering your custom callbacks

If you are registering asynchronous callbacks make sure that you respect the value of `ResilienceContext.IsSynchronous` property and execute you callbacks synchronously for synchronous executions. You should also use the `ResilienceContext.ContinueOnCapturedContext` in case your user code uses execution and synchronization context (i.e. asynchronous calls in UI applications).
