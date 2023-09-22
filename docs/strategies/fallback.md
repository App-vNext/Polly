# Fallback resilience strategy

## About

- **Options**: [`FallbackStrategyOptions<T>`](xref:Polly.Fallback.FallbackStrategyOptions`1)
- **Extensions**: `AddFallback`
- **Strategy Type**: Reactive

---

> [!NOTE]
> Version 8 documentation for this strategy has not yet been migrated. For more information on fallback concepts and behavior, refer to the [older documentation](https://github.com/App-vNext/Polly/wiki/Fallback).

## Usage

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
            return default; // returns an empty ValueTask
        }
    });
```
<!-- endSnippet -->

## Defaults

| Property         | Default Value                                                              | Description                                                                                 |
| ---------------- | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| `ShouldHandle`   | Predicate that handles all exceptions except `OperationCanceledException`. | Predicate that determines what results and exceptions are handled by the fallback strategy. |
| `FallbackAction` | `Null`, **Required**                                                       | Fallback action to be executed.                                                             |
| `OnFallback`     | `null`                                                                     | Event that is raised when fallback happens.                                                 |

## Patterns and anti-patterns

Over the years, many developers have used Polly in various ways. Some of these recurring patterns may not be ideal. This section highlights the recommended practices and those to avoid.

### 1 - Using fallback to replace thrown exception

❌ DON'T

Throw custom exceptions from the `OnFallback`:

<!-- snippet: fallback-anti-pattern-1 -->
```cs
var fallback = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddFallback(new()
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<HttpRequestException>(),
        FallbackAction = args => Outcome.FromResultAsValueTask(new HttpResponseMessage()),
        OnFallback = args => throw new CustomNetworkException("Replace thrown exception", args.Outcome.Exception!)
    })
    .Build();
```
<!-- endSnippet -->

**Reasoning**:

Throwing an exception from a user-defined delegate can disrupt the normal control flow.

✅ DO

Use `ExecuteOutcomeAsync` and then evaluate the `Exception`:

<!-- snippet: fallback-pattern-1 -->
```cs
var outcome = await WhateverPipeline.ExecuteOutcomeAsync(Action, context, "state");
if (outcome.Exception is HttpRequestException hre)
{
    throw new CustomNetworkException("Replace thrown exception", hre);
}
```
<!-- endSnippet -->

**Reasoning**:

This method lets you execute the strategy or pipeline smoothly, without unexpected interruptions. If you repeatedly find yourself writing this exception "remapping" logic, consider marking the method you wish to decorate as `private` and expose the "remapping" logic publicly.

<!-- snippet: fallback-pattern-1-ext -->
```cs
public static async ValueTask<HttpResponseMessage> Action()
{
    var context = ResilienceContextPool.Shared.Get();
    var outcome = await WhateverPipeline.ExecuteOutcomeAsync<HttpResponseMessage, string>(
        async (ctx, state) =>
        {
            var result = await ActionCore();
            return Outcome.FromResult(result);
        }, context, "state");

    if (outcome.Exception is HttpRequestException hre)
    {
        throw new CustomNetworkException("Replace thrown exception", hre);
    }

    ResilienceContextPool.Shared.Return(context);
    return outcome.Result!;
}

private static ValueTask<HttpResponseMessage> ActionCore()
{
    // The core logic
    return ValueTask.FromResult(new HttpResponseMessage());
}
```
<!-- endSnippet -->

### 2 - Using retry for fallback

Suppose you have a primary and a secondary endpoint. If the primary fails, you want to call the secondary.

❌ DON'T

Use retry for fallback:

<!-- snippet: fallback-anti-pattern-2 -->
```cs
var fallback = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new()
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .HandleResult(res => res.StatusCode == HttpStatusCode.RequestTimeout),
        MaxRetryAttempts = 1,
        OnRetry = async args =>
        {
            args.Context.Properties.Set(fallbackKey, await CallSecondary(args.Context.CancellationToken));
        }
    })
    .Build();

var context = ResilienceContextPool.Shared.Get();
var outcome = await fallback.ExecuteOutcomeAsync<HttpResponseMessage, string>(
    async (ctx, state) =>
    {
        var result = await CallPrimary(ctx.CancellationToken);
        return Outcome.FromResult(result);
    }, context, "none");

var result = outcome.Result is not null
    ? outcome.Result
    : context.Properties.GetValue(fallbackKey, default);

ResilienceContextPool.Shared.Return(context);

return result;
```
<!-- endSnippet -->

**Reasoning**:

A retry strategy by default executes the same operation up to `n` times, where `n` equals the initial attempt plus `MaxRetryAttempts`. In this case, that means **2** times. Here, the fallback is introduced as a side effect rather than a replacement.

✅ DO

Use fallback to call the secondary:

<!-- snippet: fallback-pattern-2 -->
```cs
var fallback = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddFallback(new()
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .HandleResult(res => res.StatusCode == HttpStatusCode.RequestTimeout),
        OnFallback = async args => await CallSecondary(args.Context.CancellationToken)
    })
    .Build();

return await fallback.ExecuteAsync(CallPrimary, CancellationToken.None);
```
<!-- endSnippet -->

**Reasoning**:

- The target code is executed only once.
- The fallback value is returned directly, eliminating the need for additional code like `Context` or `ExecuteOutcomeAsync`.

### 3 - Nesting `ExecuteAsync` calls

Combining multiple strategies can be achieved in various ways. However, deeply nesting `ExecuteAsync` calls can lead to what's commonly referred to as `Execute` hell.

> [!NOTE]
> While this isn't strictly tied to the Fallback mechanism, it's frequently observed when Fallback is the outermost layer.

❌ DON'T

Nest `ExecuteAsync` calls:

<!-- snippet: fallback-anti-pattern-3 -->
```cs
var result = await fallback.ExecuteAsync(async (CancellationToken outerCT) =>
{
    return await timeout.ExecuteAsync(async (CancellationToken innerCT) =>
    {
        return await CallExternalSystem(innerCT);
    }, outerCT);
}, CancellationToken.None);

return result;
```
<!-- endSnippet -->

**Reasoning**:

This is akin to JavaScript's callback hell or the pyramid of doom. It's easy to mistakenly reference the wrong `CancellationToken` parameter.

✅ DO

Use `ResiliencePipelineBuilder` to chain strategies:

<!-- snippet: fallback-pattern-3 -->
```cs
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddPipeline(timeout)
    .AddPipeline(fallback)
    .Build();

return await pipeline.ExecuteAsync(CallExternalSystem, CancellationToken.None);
```
<!-- endSnippet -->

**Reasoning**:

In this approach, we leverage the escalation mechanism provided by Polly rather than creating our own through nesting. `CancellationToken`s are automatically propagated between the strategies for you.
