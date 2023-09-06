# Fallback resilience strategy

## About

- **Options**: [`FallbackStrategyOptions<T>`](../../src/Polly.Core/Fallback/FallbackStrategyOptions.TResult.cs)
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
