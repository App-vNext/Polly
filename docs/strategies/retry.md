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
var retry = new ResiliencePipelineBuilder()
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

Use collections and simple predicate functions
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

var retry = new ResiliencePipelineBuilder()
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



❌ DON'T

Define a retry strategy to run forever in a given frequency
```cs
var retry = new ResiliencePipelineBuilder()
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

### 3 - Combining multiple sleep duration strategies

❌ DON'T

Mix the ever increasing values with constant ones
```cs
var retry = new ResiliencePipelineBuilder()
    .AddRetry(new() {
        DelayGenerator = args =>
        {
            var delay = args.AttemptNumber switch
            {
                <= 5 => TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber)),
                 _ => TimeSpan.FromMinutes(3)
            };
            return new ValueTask<TimeSpan?>(delay);
        }
    })
    .Build();
```
Reasoning:
- By changing the behaviour based on state we basically created here a state machine
- Even though it is a really compact/concise way to express sleep durations there are three main drawbacks
  - This approach does not embrace re-usability (you can't re-use only the quick retries)
  - The sleep duration logic is tightly coupled to the `AttemptNumber`
  - It is harder to unit test

✅ DO

Define two separate retry strategy options and chain them
```cs
var slowRetries = new RetryStrategyOptions {
    MaxRetryAttempts = 5,
    Delay = TimeSpan.FromMinutes(3),
    BackoffType = DelayBackoffType.Constant
};

var quickRetries = new RetryStrategyOptions {
    MaxRetryAttempts = 5,
    Delay = TimeSpan.FromSeconds(1),
    UseJitter = true,
    BackoffType = DelayBackoffType.Exponential
};

var retry = new ResiliencePipelineBuilder()
    .AddRetry(slowRetries)
    .AddRetry(quickRetries)
    .Build();
}
```
Reasoning:
- Even though this approach is a bit more verbose (compared to the previous one) it is more flexible
- You can compose the retry strategies in any order (slower is the outer and quicker is the inner or vice versa)
- You can define different triggers for the retry policies
  - Which allows you to switch back and forth between the policies based on the thrown exception or on the result
  - There is no strict order so, quick and slow retries can interleave

### 4 - Branching retry logic based on request url

Lets suppose you have an `HttpClient` and you want to decorate it with a retry only if a request is against a certain endpoint

❌ DON'T

Use `ResiliencePipeline.Empty` and `?:` operator
```cs
var retry =
    IsRetryable(req.RequestUri)
        ? new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(...).Build()
        : ResiliencePipeline<HttpResponseMessage>.Empty;
```
**Reasoning**:
- In this case the triggering conditions/logic are scattered in multiple places
- From extensibility perspective it is also not desirable since it can easily become less and less legible as you add more conditions

✅ DO

Use the `ShouldHandle` clause to define triggering logic
```cs
var retry =  new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new() {
        ShouldHandle = _ => ValueTask.FromResult(IsRetryable(req.RequestUri))
    })
    .Build();
```
**Reasoning**:
- The triggering conditions are located in a single, well-known place
- There is no need to cover _"what to do when policy shouldn't trigger"_

### 5 - Calling a given method before/after each retry attempt

❌ DON'T

Call explicitly a given method before `Execute`/`ExecuteAsync`
```cs
var retry =  new ResiliencePipelineBuilder()
    .AddRetry(new() {
        OnRetry = args => { BeforeEachAttempt(); return ValueTask.CompletedTask; },
    })
    .Build();
...
BeforeEachAttempt();
await retry.ExecuteAsync(DoSomething);
```
**Reasoning**:
- The `OnRetry` is called before each **retry** attempt.
  - So, it won't be called before the very first initial attempt (because that is not a retry)
- If this strategy is used in multiple places it is quite likely that you will forgot to call `BeforeEachAttempt` before every `Execute` calls
- Here the naming is very explicit but in real world scenario your method might not be prefixed with `Before`
  - So, one might call it after the `Execute` call which is not the intended usage

✅ DO

Decorate the two method calls together
```cs
var retry =  new ResiliencePipelineBuilder()
    .AddRetry(new() {...})
    .Build();
...
await retry.ExecuteAsync(ct => {
    BeforeEachAttempt();
    return DoSomething(ct);
});
```
**Reasoning**:
- If the `DoSomething` and `BeforeEachRetry` coupled together then decorate them together
  - Or create a simple wrapper to call them in the desired order

### 6 - Having a single policy to cover multiple failures

Lets suppose we have an `HttpClient` which issues a request and then we try to parse a large Json

❌ DON'T

Have a single policy to cover everything
```cs
var builder = new ResiliencePipelineBuilder()
    .AddRetry(new()
    {
        ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
        MaxRetryAttempts = ...
    });

builder.AddTimeout(TimeSpan.FromMinutes(1));

var pipeline = builder.Build();
await pipeline.ExecuteAsync(async (ct) =>
{
     var stream = await httpClient.GetStreamAsync(endpoint, ct);
     var foo = await JsonSerializer.DeserializeAsync<Foo>(stream, cancellationToken: ct);
     ...
});
```
**Reasoning**:
- In the previous point it was suggested that  _if the `X` and `Y` coupled together then decorate them together_
   - only if they are all part of the same failure domain
   - in other words a pipeline should cover one failure domain

✅ DO

Define a strategy per failure domain
```cs
var retry = new ResiliencePipelineBuilder()
    .AddRetry(new()
    {
        ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
        MaxRetryAttempts = ...
    })
    .Build();

var stream = await retry.ExecuteAsync((ct) => httpClient.GetStreamAsync(endpoint, ct));

var timeout = new ResiliencePipelineBuilder<Foo>()
    .AddTimeout(TimeSpan.FromMinutes(1))
    .Build();

var foo = await timeout.ExecuteAsync((ct) => JsonSerializer.DeserializeAsync<Foo>(stream, cancellationToken: ct));
```
**Reasoning**:
- Network call's failure domain is different than deserialization's failures
  - Having dedicated strategies makes the application more robust against different transient failures

### 7 - Cancelling retry in case of given exception

After you receive a `TimeoutException` you don't want to perform any more retries

❌ DON'T

Add cancellation logic inside `OnRetry`
```cs
...
.WaitAndRetryAsync(
var ctsKey = new ResiliencePropertyKey<CancellationTokenSource>("cts");
var retry =  new ResiliencePipelineBuilder()
    .AddRetry(new() {
        OnRetry = args =>
        {
            if(args.Outcome.Exception is TimeoutException)
            {
                if(args.Context.Properties.TryGetValue(ctsKey, out var cts))
                {
                    cts.Cancel();
                }
            }
            return ValueTask.CompletedTask;
        },
        ...
    })
    .Build();
```
**Reasoning**:
- The triggering logic/conditions should be placed inside `ShouldHandle`
- "Jumping out from a strategy" from a user-defined delegate either via an `Exception` or by a `CancellationToken` just complicates the control flow unnecessarily

✅ DO

Define triggering logic inside `ShouldHandle`
```cs
...
var retry =  new ResiliencePipelineBuilder()
    .AddRetry(new() {
        ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is not TimeoutException),
        ...
    })
    .Build();
```
**Reasoning**:
- As it was stated above please use the dedicated place to define triggering condition
   - Try to rephrase your original exit condition in a way to express _when should a retry trigger_
