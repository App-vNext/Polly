# General

## Supported targets

Polly targets .NET Standard 2.0+ ([coverage](https://docs.microsoft.com/dotnet/standard/net-standard#net-implementation-support): .NET Core 2.0+, .NET Core 3.0, .NET 6.0+ and later Mono, Xamarin and UWP targets). The NuGet package also includes direct targets for .NET Framework 4.6.1 and 4.7.2.

For details of supported compilation targets by version, see the [supported targets](https://github.com/App-vNext/Polly/wiki/Supported-targets) grid.

## Asynchronous support

Polly provides native support for asynchronous operations through all its resilience strategies by offering the `ExecuteAsync` methods on the `ResiliencePipeline` class.

### SynchronizationContext

By default, asynchronous continuations and retries do not execute on a captured synchronization context. To modify this behavior, you can use the `ResilienceContext` class and set its `ContinueOnCapturedContext` property to `true`. The following example illustrates this:

<!-- snippet: synchronization-context -->
```cs
// Retrieve an instance of ResilienceContext from the pool
// with the ContinueOnCapturedContext property set to true
ResilienceContext context = ResilienceContextPool.Shared.Get(continueOnCapturedContext: true);

await pipeline.ExecuteAsync(
    async context =>
    {
        // Execute your code, honoring the ContinueOnCapturedContext setting
        await MyMethodAsync(context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);
    },
    context);

// Optionally, return the ResilienceContext instance back to the pool
// to minimize allocations and enhance performance
ResilienceContextPool.Shared.Return(context);
```
<!-- endSnippet -->

### Cancellation support

Asynchronous pipeline execution in Polly supports cancellation. This is facilitated through the `ExecuteAsync(...)` method overloads that accept a `CancellationToken`, or by initializing the `ResilienceContext` class with the `CancellationToken` property.

The `CancellationToken` you pass to the `ExecuteAsync(...)` method serves multiple functions:

- It cancels resilience actions such as retries, wait times between retries, or rate-limiter leases.
- It is passed to any delegate executed by the strategy as a `CancellationToken` parameter, enabling cancellation during the delegate's execution.
- Is consistent with the .NET Base Class Library's (BCL) behavior in `Task.Run(...)`, if the cancellation token is cancelled before execution begins, the user-defined delegate will not execute at all.

<!-- snippet: cancellation-token -->
```cs
// Execute your code with cancellation support
await pipeline.ExecuteAsync(
    async token => await MyMethodAsync(token),
    cancellationToken);

// Use ResilienceContext for more advanced scenarios
ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken: cancellationToken);

await pipeline.ExecuteAsync(
    async context => await MyMethodAsync(context.CancellationToken),
    context);
```
<!-- endSnippet -->

## Thread safety

All Polly resilience strategies are fully thread-safe. You can safely re-use strategies at multiple call sites, and execute through strategies concurrently on different threads.

> [!IMPORTANT]
> While the internal operation of the strategy is thread-safe, this does not automatically make delegates you execute through the strategy thread-safe: if delegates you execute through the strategy are not thread-safe, they remain not thread-safe.
