# Resilience strategies

Resilience strategies are essential components of Polly, designed to execute user-defined callbacks while adding an extra layer of resilience. These strategies can't be executed directly; they must be run through a **resilience pipeline**. Polly provides an API to construct resilience pipelines by incorporating one or more resilience strategies through the pipeline builders.

Polly categorizes resilience strategies into two main groups:

- **Reactive**: These strategies handle specific exceptions that are thrown, or results that are returned, by the callbacks executed through the strategy.
- **Proactive**: Unlike reactive strategies, proactive strategies do not focus on handling errors by the callbacks might throw or return. They can make proactive decisions to cancel or reject the execution of callbacks (e.g., using a rate limiter or a timeout resilience strategy).

## Built-in strategies

### Reactive

| Strategy | Premise | AKA | How does the strategy mitigate? |
| -------- | ------- | :-: | ------------- |
| [Retry](retry.md) | Many faults are transient and may self-correct after a short delay. | *Maybe it's just a blip* | Allows configuring automatic retries. |
| [Circuit-breaker](circuit-breaker.md) | When a system is seriously struggling, failing fast is better than making users/callers wait.  <br/><br/>Protecting a faulting system from overload can help it recover. | *Stop doing it if it hurts* <br/><br/>*Give that system a break* | Breaks the circuit (blocks executions) for a period, when faults exceed some pre-configured threshold. |
| [Fallback](fallback.md) | Things will still fail - plan what you will do when that happens. | *Degrade gracefully* | Defines an alternative value to be returned (or action to be executed) on failure. |
| [Hedging](hedging.md) | Things can be slow sometimes, plan what you will do when that happens. | *Hedge your bets* | Executes parallel actions when things are slow and waits for the fastest one. |

### Proactive

| Strategy | Premise | AKA | How does the strategy prevent? |
| -------- | ------- | :-: | ------------- |
| [Timeout](timeout.md) | Beyond a certain wait, a success result is unlikely. | *Don't wait forever* | Guarantees the caller won't have to wait beyond the timeout. |
| [Rate Limiter](rate-limiter.md) | Limiting the rate a system handles requests is another way to control load. <br/><br/> This can apply to the way your system accepts incoming calls, and/or to the way you call downstream services. | *Slow down a bit, will you?* |Constrains executions to not exceed a certain rate. |

## Usage

Extensions for adding resilience strategies to the builders are provided by each strategy. Depending on the type of strategy, these extensions may be available for both `ResiliencePipelineBuilder` and `ResiliencePipelineBuilder<T>` or just for the latter one. Adding multiple resilience strategies is supported.

| Strategy | `ResiliencePipelineBuilder` | `ResiliencePipelineBuilder<T>` |
| -------- | :-------------------------: | :-------------: |
| Circuit Breaker | ✅ | ✅ |
| Fallback | ❌ | ✅ |
| Hedging | ❌ | ✅ |
| Rate Limiter | ✅ | ✅ |
| Retry | ✅ | ✅ |
| Timeout | ✅ | ✅ |

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

## Fault handling

Each reactive strategy provides access to the `ShouldHandle` predicate property. This property offers a mechanism to decide whether the resilience strategy should manage the fault or result returned after execution.

Setting up the predicate can be accomplished in the following ways:

- **Manually setting the predicate**: Directly configure the predicate. The advised approach involves using [switch expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/switch-expression) for maximum flexibility, and also allows the incorporation of asynchronous predicates.
- **Employing `PredicateBuilder`**: The `PredicateBuilder{<TResult>}` classes provide a more straight-forward method to configure the predicates, akin to predicate setups in earlier Polly versions.

The examples below illustrate these:

### Predicates

<!-- snippet: should-handle-manual -->
```cs
var options = new RetryStrategyOptions<HttpResponseMessage>
{
    // For greater flexibility, you can directly use the ShouldHandle delegate with switch expressions.
    ShouldHandle = args => args.Outcome switch
    {
        { Exception: HttpRequestException } => PredicateResult.True(),
        { Exception: TimeoutRejectedException } => PredicateResult.True(), // You can handle multiple exceptions
        { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
        _ => PredicateResult.False()
    }
};
```
<!-- endSnippet -->

Notes from the preceding example:

- Switch expressions are used to determine whether to retry or not.
- `PredicateResult.True()` is a shorthand for `new ValueTask<bool>(true)`.
- `ShouldHandle` predicates are asynchronous and use the type `Func<Args<TResult>, ValueTask<bool>>`.
  - The `Args<TResult>` acts as a placeholder, and each strategy defines its own arguments.
- Multiple exceptions can be handled using switch expressions.

> [!NOTE]
> The `args` parameter of the `ShouldHandle` allows read-only access to strategy specific information.
> For example in case of retry you can access the `AttemptNumber`, as well as the `Outcome` and `Context`.

### Asynchronous predicates

You can also use asynchronous delegates for more advanced scenarios, such as retrying based on the response body:

<!-- snippet: should-handle-manual-async -->
```cs
var options = new RetryStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = async args =>
    {
        if (args.Outcome.Exception is not null)
        {
            return args.Outcome.Exception switch
            {
                HttpRequestException => true,
                TimeoutRejectedException => true,
                _ => false
            };
        }

        // Determine whether to retry asynchronously or not based on the result.
        return await ShouldRetryAsync(args.Outcome.Result!, args.Context.CancellationToken);
    }
};
```
<!-- endSnippet -->

### Predicate builder

<xref:Polly.PredicateBuilder>, or <xref:Polly.PredicateBuilder`1>, is a utility class aimed at simplifying the configuration of predicates:

<!-- snippet: should-handle-predicate-builder -->
```cs
// Use PredicateBuilder<HttpResponseMessage> to simplify the setup of the ShouldHandle predicate.
var options = new RetryStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .HandleResult(response => !response.IsSuccessStatusCode) // Handle results
        .Handle<HttpRequestException>() // Or handle exception
        .Handle<TimeoutRejectedException>() // Chaining is supported
};
```
<!-- endSnippet -->

The preceding sample:

- Uses `HandleResult` to register a predicate that determines whether the result should be handled or not.
- Uses `Handle` to handle multiple exceptions types.

> [!NOTE]
> When using `PredicateBuilder` instead of manually configuring the predicate, there is a minor performance impact. Each method call on `PredicateBuilder` registers a new predicate, which must be invoked when evaluating the outcome.
