# Retry resilience strategy

## About

- **Options**:
  - [`RetryStrategyOptions`](xref:Polly.Retry.RetryStrategyOptions)
  - [`RetryStrategyOptions<T>`](xref:Polly.Retry.RetryStrategyOptions`1)
- **Extensions**: `AddRetry`
- **Strategy Type**: Reactive

---

> [!NOTE]
> Version 8 documentation for this strategy has not yet been migrated. For more information on retry concepts and behavior, refer to the [older documentation](https://github.com/App-vNext/Polly/wiki/Retry).

## Usage

<!-- snippet: Retry -->
```cs
// Add retry using the default options.
// See https://www.pollydocs.org/strategies/retry#defaults for defaults.
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions());

// For instant retries with no delay
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    Delay = TimeSpan.Zero
});

// For advanced control over the retry behavior, including the number of attempts,
// delay between retries, and the types of exceptions to handle.
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true,  // Adds a random factor to the delay
    MaxRetryAttempts = 4,
    Delay = TimeSpan.FromSeconds(3),
});

// To use a custom function to generate the delay for retries
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = 2,
    DelayGenerator = args =>
    {
        var delay = args.AttemptNumber switch
        {
            0 => TimeSpan.Zero,
            1 => TimeSpan.FromSeconds(1),
            _ => TimeSpan.FromSeconds(5)
        };

        // This example uses a synchronous delay generator,
        // but the API also supports asynchronous implementations.
        return new ValueTask<TimeSpan?>(delay);
    }
});

// To extract the delay from the result object
new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
{
    DelayGenerator = args =>
    {
        if (args.Outcome.Result is HttpResponseMessage responseMessage &&
            TryGetDelay(responseMessage, out TimeSpan delay))
        {
            return new ValueTask<TimeSpan?>(delay);
        }

        // Returning null means the retry strategy will use its internal delay for this attempt.
        return new ValueTask<TimeSpan?>((TimeSpan?)null);
    }
});

// To get notifications when a retry is performed
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = 2,
    OnRetry = args =>
    {
        Console.WriteLine("OnRetry, Attempt: {0}", args.AttemptNumber);

        // Event handlers can be asynchronous; here, we return an empty ValueTask.
        return default;
    }
});

// To keep retrying indefinitely or until success use int.MaxValue.
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = int.MaxValue,
});
```
<!-- endSnippet -->

## Defaults

| Property           | Default Value                                                              | Description                                                                              |
| ------------------ | -------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| `ShouldHandle`     | Predicate that handles all exceptions except `OperationCanceledException`. | Predicate that determines what results and exceptions are handled by the retry strategy. |
| `MaxRetryAttempts` | 3                                                                          | The maximum number of retries to use, in addition to the original call.                  |
| `Delay`            | 2 seconds                                                                  | The base delay between retries.                                                          |
| `BackoffType`      | Constant                                                                   | The type of the back-off used to generate the retry delay.                               |
| `UseJitter`        | False                                                                      | Allows adding jitter to retry delays.                                                    |
| `DelayGenerator`   | `null`                                                                     | Used for generating custom delays for retries.                                           |
| `OnRetry`          | `null`                                                                     | Action executed when retry occurs.                                                       |

## Patterns and Anti-patterns
Throughout the years many people have used Polly in so many different ways. Some reoccuring patterns are suboptimal. So, this section shows the donts and dos.

### 1 - Overusing builder methods

❌ DON'T
Use more than one `Handle/HandleResult`
```cs
var retryPolicy = new ResiliencePipelineBuilder()
    .AddRetry(new() {
        ShouldHandle = new PredicateBuilder()
        .Handle<HttpRequestException>()
        .Handle<BrokenCircuitException>()
        .Handle<TimeoutRejectedException>()
        .Handle<SocketException>()
        .Handle<RateLimitRejectedException>(),
        MaxRetryAttempts = ...,
    })
    .Build();
```
**Reasoning**:
- Even though this builder method signature is quite concise you repeat the same thing over and over (_please trigger retry if the to-be-decorated code throws XYZ exception_).
- A better approach would be to tell  _please trigger retry if the to-be-decorated code throws one of the retriable exceptions_.

✅ DO
Use collections and predicates
```cs
ImmutableArray<Type> networkExceptions = new[] {
    typeof(SocketException),
    typeof(HttpRequestException),
}.ToImmutableArray();

ImmutableArray<Type> policyExceptions = new[] {
    typeof(TimeoutRejectedException),
    typeof(BrokenCircuitException),
    typeof(RateLimitRejectedException),
}.ToImmutableArray();

var retryPolicy = new ResiliencePipelineBuilder()
    .AddRetry(new() {
        ShouldHandle = ex => new ValueTask<bool>(
            networkExceptions.Union(policyExceptions).Contains(ex.GetType())),
        MaxRetryAttempts = ...,
    })
    .Build();
```
**Reasoning**:
- This approach embraces re-usability.
  - For instance the `networkExceptions` can be reused across many the strategies (retry, circuit breaker, etc..).

### 2 - Using retry as a periodical executor

The retry strategy can be defined in a way to run forever in a given frequency.

❌ DON'T
```cs
var retryForeverDaily = new ResiliencePipelineBuilder()
    .AddRetry(new()
    {
        ShouldHandle = _ => ValueTask.FromResult(true),
        Delay = TimeSpan.FromHours(24),
    })
    .Build();
```
**Reasoning**:
- The sleep period can be blocking or non-blocking depending on how you define your strategy/pipeline.
- Even if it is used in a non-blocking manner it consumes (_unnecessarily_) memory which can't be garbage collected.

✅ DO
Use appropriate tool to schedule recurring jobs like *Quartz.Net*, *Hangfire* or similar.

**Reasoning**:
- Polly was never design to support this use case rather than its main aim is to help you overcome **short** transient failures.
- Dedicated job scheduler tools are more efficient (in terms of memory) and can be configured to withstand machine failure by utilizing persistence storage.
