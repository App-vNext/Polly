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

## Handling of different result types

Various implementations of `ResilienceStrategy` use callbacks to provide or request information from user. The callbacks are generic and support any type of result. Most strategies will use the following types of callbacks:

- **Predicates**: These return `true` or `false` values based on the input. The input can be the result of an user-callback or some exception. For example, to determine whether we should retry the user-callback for a specific result.
- **Events**: These are events raised when something important happens. For example when a timeout occurs.
- **Generators**: These generate a value based on the input. For example, a retry delay before the next retry attempt.

All callbacks are asynchronous and return `ValueTask`. They provide the following information to the user:

- `ResilienceContext`: the context of the operation.
- Result type: for what result type is the strategy being executed.
- Callback arguments: Additional information about the event. Using arguments is preferable because it makes the API more stable. If we decide to add a new member to the arguments, the call sites won't break.

Each callback type has an associated class that can be reused across various strategies. For example, see the `Predicates` class and the usage of the `RetryStrategyOptions.ShouldRetry` property:

``` csharp
public Predicates ShouldRetry { get; set; } = new();
```

``` csharp
var options = new RetryStrategyOptions();
options
    .ShouldRetry
    .Add<HttpResponseMessage>(result => result.StatusCode == HttpStatusCode.InternalServerError) // inspecting the result
    .Add(HttpStatusCode.InternalServerError) // particular value for other type
    .Add<MyResult>(result => result.IsError)
    .Add<MyResult>((result, context) => IsError(context)) // retrieve data from context for evaluation
    .AddException<InvalidOperationException>() // exceptions
    .AddException<HttpRequestMessageException>() // more exceptions
    .AddException(error => IsError(error)) // exception predicates
    .Add<MyResult>(async (result, context) => await IsErrorAsync(result, context)); // async predicates
```

In the preceding sample you see that `ShouldRetry` handles the following scenarios:

- Asynchronous predicates;
- Synchronous predicates;
- Concrete value results;
- Custom function-based callbacks;
- Different result types;
- Exception types or exception-based predicates;
