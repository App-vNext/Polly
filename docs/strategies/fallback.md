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


## Patterns and Anti-patterns
Throughout the years many people have used Polly in so many different ways. Some reoccuring patterns are suboptimal. So, this section shows the donts and dos.

### 1 - Using fallback to replace thrown exception

❌ DON'T

Throw custom exception from the `OnFallback`

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
- Throwing an exception in an user-defined delegate is never a good idea because it is breaking the normal control flow.

✅ DO

Use `ExecuteOutcomeAsync` and then assess `Exception`

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
- This approach executes the strategy/pipeline without "jumping out from the normal flow"
- If you find yourself in a situation that you write this Exception "remapping" logic again and again
  - then mark the to-be-decorated method as `private`
  - and expose the "remapping" logic as `public`

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

### 2 - Using retry to perform fallback

Lets suppose you have a primary and a secondary endpoints. If primary fails then you want to call the secondary.

❌ DON'T

Use retry to perform fallback

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
- Retry strategy by default executes the exact same operation at most `n` times
  - where `n` equals to the initial attempt + `MaxRetryAttempts`
  - So, in this particular case this means __2__
- Here the fallback is produced as a side-effect rather than as a substitute

✅ DO

Use fallback to call secondary

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
- The to-be-decorated code is executed only once
- The fallback value will be returned without any extra code (no need for `Context` or `ExecuteOutcomeAsync`)

### 3 - Nesting `ExecuteAsync` calls

There are many ways to combine multiple strategies together. One of the least desired one is the `Execute` hell.

> [!NOTE]
> This is not strictly related to Fallback but we have seen it many times when Fallback was the most outer.

❌ DON'T

Nest `ExecuteAsync` calls

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
- This is the same as javascript's callback hell or pyramid of doom
- It is pretty easy to refer to the wrong `CancellationToken` parameter

✅ DO
Use `ResiliencePipelineBuilder` to chain them

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
- Here we are relying Polly provided escalation mechanism rather than building our own via nesting
- The `CancellationToken`s are propagated between the policies automatically on your behalf
