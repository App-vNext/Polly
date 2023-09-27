# Resilience strategies

Resilience strategies are essential components of Polly, designed to execute user-defined callbacks while adding an extra layer of resilience. These strategies can't be executed directly; they must be run through a **resilience pipeline**. Polly provides an API to construct resilience pipelines by incorporating one or more resilience strategies through the pipeline builders.

Polly categorizes resilience strategies into two main groups:

- **Reactive**: These strategies handle specific exceptions that are thrown, or results that are returned, by the callbacks executed through the strategy.
- **Proactive**: Unlike reactive strategies, proactive strategies do not focus on handling errors by the callbacks might throw or return. They can make proactive decisions to cancel or reject the execution of callbacks (e.g., using a rate limiter or a timeout resilience strategy).

## Built-in strategies

| Strategy | Reactive | Premise | AKA | How does the strategy mitigate?|
| ------------- | --- | ------------- |:-------------: |------------- |
|[Retry](retry.md) |Yes|Many faults are transient and may self-correct after a short delay.| *Maybe it's just a blip* |  Allows configuring automatic retries. |
|[Circuit-breaker](circuit-breaker.md) |Yes|When a system is seriously struggling, failing fast is better than making users/callers wait.  <br/><br/>Protecting a faulting system from overload can help it recover. | *Stop doing it if it hurts* <br/><br/>*Give that system a break* | Breaks the circuit (blocks executions) for a period, when faults exceed some pre-configured threshold. |
|[Timeout](timeout.md)|No|Beyond a certain wait, a success result is unlikely.| *Don't wait forever*  |Guarantees the caller won't have to wait beyond the timeout. |
|[Rate Limiter](rate-limiter.md)|No|Limiting the rate a system handles requests is another way to control load. <br/><br/> This can apply to the way your system accepts incoming calls, and/or to the way you call downstream services. | *Slow down a bit, will you?*  |Constrains executions to not exceed a certain rate. |
|[Fallback](fallback.md)|Yes|Things will still fail - plan what you will do when that happens.| *Degrade gracefully*  |Defines an alternative value to be returned (or action to be executed) on failure. |
|[Hedging](hedging.md)|Yes|Things can be slow sometimes, plan what you will do when that happens.| *Hedge your bets*  | Executes parallel actions when things are slow and waits for the fastest one.  |

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
