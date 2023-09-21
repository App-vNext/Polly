# Polly V8 API Documentation

The Polly V8 API offers a unified, non-allocating resilience API. At the core of Polly V8 is the [`ResiliencePipeline`](ResiliencePipeline.cs) class, responsible for executing user-provided callbacks. This class handles all scenarios covered in Polly V7, such as:

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

## Resources

Visit <https://pollydocs.org> to learn more about Polly.
