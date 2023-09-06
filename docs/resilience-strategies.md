# Resilience Strategies

Resilience strategies are essential components of Polly, designed to execute user-defined callbacks while adding an extra layer of resilience. These strategies can't be executed directly; they must be run through a **resilience pipeline**. Polly provides an API to construct resilience pipelines by incorporating one or more resilience strategies through the pipeline builders.

## Usage

Extensions for adding resilience strategies to the builders are provided by each strategy. Depending on the type of strategy, these extensions may be available for both `ResiliencePipelineBuilder` and `ResiliencePipelineBuilder<T>` or just one of them. Proactive strategies like timeout or rate limiter are available for both types of builders, while specialized reactive strategies are only available for `ResiliencePipelineBuilder<T>`. Adding multiple resilience strategies is supported.

Each resilience strategy provides:

- Extensions for the resilience strategy builders.
- Configuration options (e.g., `RetryStrategyOptions`) to specify the strategy's behavior.

Here's an simple example:

<!-- snippet: resilience-strategy-sample -->
```cs
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(5)
    })
    .Build();
```
<!-- endSnippet -->

> [!NOTE]
> The configuration options are automatically validated by Polly and come with sensible defaults. Therefore, you don't have to specify all the properties unless needed.

## Fault-handling in reactive strategies

Each reactive strategy exposes the `ShouldHandle` predicate property. This property represents a predicate to determine whether the fault or the result returned after executing the resilience strategy should be managed or not.

This is demonstrated below:

<!-- snippet: should-handle -->
```cs
// Create an instance of options for a retry strategy. In this example,
// we use RetryStrategyOptions. You could also use other options like
// CircuitBreakerStrategyOptions or FallbackStrategyOptions.
var options = new RetryStrategyOptions<HttpResponseMessage>();

// PredicateBuilder can simplify the setup of the ShouldHandle predicate.
options.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
    .HandleResult(response => !response.IsSuccessStatusCode)
    .Handle<HttpRequestException>();

// For greater flexibility, you can directly use the ShouldHandle delegate with switch expressions.
options.ShouldHandle = args => args.Outcome switch
{
    // Strategies may offer additional context for result handling.
    // For instance, the retry strategy exposes the number of attempts made.
    _ when args.AttemptNumber > 3 => PredicateResult.False(),
    { Exception: HttpRequestException } => PredicateResult.True(),
    { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
    _ => PredicateResult.False()
};
```
<!-- endSnippet -->

Some additional notes from the preceding example:

- `PredicateBuilder` is a utility API designed to make configuring predicates easier.
- `PredicateResult.True()` is shorthand for `new ValueTask<bool>(true)`.
- All `ShouldHandle` predicates are asynchronous and have the type `Func<Args<TResult>, ValueTask<bool>>`. The `Args<TResult>` serves as a placeholder, and each strategy defines its own arguments.
