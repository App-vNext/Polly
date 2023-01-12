# Introduction

The Polly V8 demo is a proof of concept (POC) showcase of how the unified and non-allocating resilience API can look like.

## API

At the heart of the POC is the `IResilienceStrategy` interface that is responsible for execution of user code. It's one interface that handles all Polly scenarios:

- `ISyncPolicy`
- `IAsyncPolicy`
- `ISyncPolicy<T>`
- `IAsyncPolicy<T>`

``` csharp
public interface IResilienceStrategy
{
    ValueTask<T> ExecuteAsync<T, TState>(Func<ResilienceContext, TState, ValueTask<T>> execution, ResilienceContext context, TState state);
}
```

The `ResilienceContext` is defined as:

``` csharp
public class ResilienceContext
{
    public CancellationToken CancellationToken { get; set; }

    public bool IsSynchronous { get; set; }

    public bool IsVoid { get; set; }

    public bool ContinueOnCapturedContext { get; set; }

    // omitted for simplicity
}
```

The `IResilienceStrategy` unifies the 4 different policies used now in Polly. User actions are executed under a single API. The are many extension
methods for this interface that cover different scenarios:

- Synchronous void methods.
- Synchronous methods with result.
- Asynchronous void methods.
- Asynchronous methods with result.

For example, synchronous `Execute` extension:

``` csharp
public static void Execute(this IResilienceStrategy strategy, Action execute)
{
    var context = ResilienceContext.Get();
    context.IsSynchronous = true;
    context.IsVoid = true;

    try
    {
        strategy.ExecuteAsync(static (context, state) =>
        {
            state();
            return new ValueTask<VoidResult>(VoidResult.Instance);
        }, 
        context, execute).GetAwaiter().GetResult();
    }
    finally
    {
        ResilienceContext.Return(context);
    }
}
```

In the preceding example:

- We rent `ResilienceContext` from pool.
- We store the information about the execution mode by setting the `IsSynchronous` and `IsVoid` properties to the context.
- We pass the user delegate, and use the `State` to avoid closure allocation.
- We block the execution.
- We return `ResilienceContext` to the pool.

Underlying implementation decides how to execute this delegate by reading the `ResilienceContext`:

``` csharp
internal class DelayStrategy : DelegatingResilienceStrategy
{
    public async override ValueTask<T> ExecuteAsync<T, TState>(Func<ResilienceContext, TState, ValueTask<T>> execution, ResilienceContext context, TState state)
    {
        if (context.IsSynchronous)
        {
            Thread.Sleep(1000);
        }
        else
        {
            await Task.Delay(1000);
        }

        return await execution(context, state);
    }
}
```

In the preceding example:

- For synchronous execution we are using `Thread.Sleep`.
- For asynchronous execution we are using `Task.Delay`.

This way, the responsibility of how to execute method is lifted from the user and instead passed to the policy. User knows only the `IResilienceStrategy` interface. User uses only a single strategy to execute all scenarios. Previously, user had to decide whether to use sync vs async, typed vs non-typed policies.

The life of extensibility author is also simplified as he only maintains one implementation of strategy instead of multiple ones. See the duplications in [`Polly.Retry`](https://github.com/App-vNext/Polly/tree/master/src/Polly/Retry).

### Creation of `IResilienceStrategy`

This API exposes the following classes and interfaces that can be used to create the resilience strategy:

- `IResilienceStrategyBuilder`
- `ResilienceStrategyBuilder`: concrete implementation of `IResilienceStrategyBuilder`.

``` csharp
public interface IResilienceStrategyBuilder
{
    ResilienceStrategyBuilderOptions Options { get; set; }

    IResilienceStrategyBuilder AddStrategy(IResilienceStrategy strategy, ResilienceStrategyOptions? options = null);

    IResilienceStrategyBuilder AddStrategy(Func<ResilienceStrategyBuilderContext, IResilienceStrategy> factory, ResilienceStrategyOptions? options = null);

    IResilienceStrategy Create();
}
```

To create a strategy you chain various extensions for `IResilienceStrategyBuilder` followed by the `Create` call:

Single strategy:

``` csharp
var resilienceStrategy = new ResilienceStrategyBuilder().AddRetry().Create();
```

Pipeline of strategies:

``` csharp
var resilienceStrategy = new ResilienceStrategyBuilder()
    .AddRetry()
    .AddCircuitBreaker()
    .AddTimeout(new TimeoutStrategyOptions() { ... })
    .Create();
```

### Extensibility

The resilience extensibility is simple. You just expose new extensions for `IResilienceStrategyBuilder` that use the `IResilienceStrategyBuilder.AddStrategy` methods.

### Handling of different result types

The resilience strategy can handle many different result types and exceptions as retry strategy sample demonstrates:

``` csharp
var options = new RetryStrategyOptions();
options
    .ShouldRetry
    .Add<HttpResponseMessage>(m => m.StatusCode == HttpStatusCode.InternalServerError) // inspecting the result
    .Add(HttpStatusCode.InternalServerError) // particular value for other type
    .Add<MyResult>(v => v.IsError)
    .Add<MyResult>((v, context) => IsError(context)) // retrieve data from context for evaluation
    .AddException<InvalidOperationException>() // exceptions
    .AddException<HttpRequestMessageException>() // more exceptions
    .Add<MyResult>((v, context) => await IsErrorAsync(v, context)); // async predicates
```

In the preceding sample retry strategy handles 3 different types of results. This allows sharing of the retry strategy across the different result types. It is also possible to retrieve additional details from the `ResilienceContext` when handling the result.

### Packages

All packages have the same `Polly` root namespace.

#### Polly.Abstractions

Contains core primitives for consumers that just want to accept the core `IResilienceStrategy` in their APIs.

**Dependencies:** None

**Types**:

- `IResilienceStrategy`
- `ResilienceStrategyExtensions`
- `Context`
- `NullResilienceStrategy`
- `DelegatingResilienceStrategy`

#### Polly.Core

Contains the builder for resilience strategies and core primitives that can be reused across various strategies.

**Dependencies:**

- `Poly.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`

**Types**:

- `IResilienceStrategyBuilder`
- `ResilienceStrategyBuilder`
- `ResilienceStrategyOptions`
- `ResilienceStrategyBuilderOptions`
- `ResilienceStrategyBuilderContext`

#### Polly.Strategies

Contains individual built-in strategies.

**Dependencies:**

- `Poly.Core`

**Types**:

- `RetryStrategyOptions`
- `TimeoutStrategyOptions`
- `RetryExtensions`
- `TimeoutExtensions`

#### Polly.DependencyInjection

Contains DI support for the `IServiceCollection`.

**Dependencies:**

- `Poly.Core`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Options`

**Types**:

- `ResilienceServiceCollectionExtensions`
- `ResilienceStrategyContext`
- `IResilienceStrategyProvider`
