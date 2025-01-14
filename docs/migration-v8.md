# Migration guide from v7 to v8

Welcome to the migration guide for Polly's v8 release. Version 8 of Polly brings major new enhancements and supports all of the same scenarios as previous versions. In the following sections, we'll detail the differences between the v7 and v8 APIs, and provide steps on how to transition smoothly.

> [!NOTE]
> The v7 API is still available and fully supported even when using the v8 version by referencing the [Polly](https://www.nuget.org/packages/Polly) package.

## Major differences

- **The term *Policy* is now replaced with *Strategy***: In previous versions, Polly used the term *policy* for retries, timeouts, etc. In v8, these are referred to as *resilience strategies*.
- **Introduction of Resilience Pipelines**: A [resilience pipeline](pipelines/index.md) combines one or more resilience strategies. This is the foundational API for Polly v8, similar to the **Policy Wrap** in previous versions but integrated into the core API.
- **Unified sync and async flows**: Interfaces such as `IAsyncPolicy`, `IAsyncPolicy<T>`, `ISyncPolicy`, `ISyncPolicy<T>`, and `IPolicy` are now unified under `ResiliencePipeline` and `ResiliencePipeline<T>`. The resilience pipeline supports both synchronous and asynchronous execution flows.
- **Native async support**: Polly v8 was designed with asynchronous support from the start.
- **No static APIs**: Unlike previous versions, v8 doesn't use static APIs. This improves testability and extensibility while maintaining ease of use.
- **Options-based configuration**: Configuring individual resilience strategies is now options-based, offering more flexibility and improving maintainability and extensibility.
- **Built-in telemetry**: Polly v8 now has built-in telemetry support.
- **Improved performance and low-allocation APIs**: Polly v8 brings significant performance enhancements and provides zero-allocation APIs for advanced use cases.

> [!NOTE]
> Please read the comments in the code carefully for additional context and explanations.

## Polly or Polly.Core package

When you do your migration process it is recommended to follow these steps:

- Upgrade the `Polly` package version from 7.x to 8.x
  - Your previous policies should run smoothly without any change
- Migrate your V7 policies to V8 strategies gradually, such as one at a time
  - Test your migrated code thoroughly
- After you have successfully migrated all your legacy Polly code then change your package reference from `Polly` to [`Polly.Core`](https://www.nuget.org/packages/Polly.Core)

## Migrating execution policies

This section describes how to migrate from execution policies (i.e. `IAsyncPolicy`, `ISyncPolicy`) to resilience pipelines (i.e. `ResiliencePipeline`, `ResiliencePipeline<T>`).

### Configuring policies in v7

In earlier versions, Polly exposed various interfaces to execute user code:

- `IAsyncPolicy`
- `IAsyncPolicy<T>`
- `ISyncPolicy`
- `ISyncPolicy<T>`

These interfaces were created and used as shown below:

<!-- snippet: migration-policies-v7 -->
```cs
// Create and use the ISyncPolicy.
ISyncPolicy syncPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));

syncPolicy.Execute(() =>
{
    // Your code goes here
});

// Create and use the IAsyncPolicy
IAsyncPolicy asyncPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));
await asyncPolicy.ExecuteAsync(async token =>
{
    // Your code goes here
}, cancellationToken);

// Create and use the ISyncPolicy<T>
ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy<HttpResponseMessage>
    .HandleResult(result => !result.IsSuccessStatusCode)
    .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));

syncPolicyT.Execute(() =>
{
    // Your code goes here
    return GetResponse();
});

// Create and use the IAsyncPolicy<T>
IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy<HttpResponseMessage>
    .HandleResult(result => !result.IsSuccessStatusCode)
    .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));

await asyncPolicyT.ExecuteAsync(async token =>
{
    // Your code goes here
    return await GetResponseAsync(token);
}, cancellationToken);
```
<!-- endSnippet -->

### Configuring strategies in v8

In Polly v8, there are no such interfaces. The previous samples become:

<!-- snippet: migration-policies-v8 -->
```cs
// Create and use the ResiliencePipeline.
//
// Use the ResiliencePipelineBuilder to start building the resilience pipeline
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        Delay = TimeSpan.FromSeconds(1),
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Constant
    })
    .Build(); // After all necessary strategies are added, call Build() to create the pipeline.

// Synchronous execution
pipeline.Execute(static () =>
{
    // Your code goes here
});

// Asynchronous execution is also supported with the same pipeline instance
await pipeline.ExecuteAsync(static async token =>
{
    // Your code goes here
}, cancellationToken);

// Create and use the ResiliencePipeline<T>.
//
// Building of generic resilience pipeline is very similar to non-generic one.
// Notice the use of generic RetryStrategyOptions<HttpResponseMessage> to configure the strategy.
ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<Exception>()
            .HandleResult(static result => !result.IsSuccessStatusCode),
        Delay = TimeSpan.FromSeconds(1),
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Constant
    })
    .Build();

// Synchronous execution
pipelineT.Execute(static () =>
{
    // Your code goes here
    return GetResponse();
});

// Asynchronous execution
await pipelineT.ExecuteAsync(static async token =>
{
    // Your code goes here
    return await GetResponseAsync(token);
}, cancellationToken);
```
<!-- endSnippet -->

> [!TIP]
>
> Things to remember:
>
> - Use `ResiliencePipelineBuilder{<TResult>}` to build a resiliency pipeline
> - Use one of the `Add*` builder methods to add a new strategy to the pipeline
> - Use either `Execute` or `ExecuteAsync` depending on the execution context
>
> For further information please check out the [Resilience pipelines documentation](pipelines/index.md).

## Migrating policy wrap

### Policy wrap in v7

Policy wrap is used to combine multiple policies into one:

<!-- snippet: migration-policy-wrap-v7 -->
```cs
IAsyncPolicy retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));

IAsyncPolicy timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(3));

// Wrap the policies. The policies are executed in the following order:
// 1. Retry <== outer
// 2. Timeout <== inner
IAsyncPolicy wrappedPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);
```
<!-- endSnippet -->

### Policy wrap in v8

In v8, there's no need to use policy wrap explicitly. Instead, policy wrapping is integrated into `ResiliencePipelineBuilder`:

<!-- snippet: migration-policy-wrap-v8 -->
```cs
// The "PolicyWrap" is integrated directly. The strategies are executed in the following order:
// 1. Retry <== outer
// 2. Timeout <== inner
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new()
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Constant,
        ShouldHandle = new PredicateBuilder().Handle<Exception>()
    })
    .AddTimeout(TimeSpan.FromSeconds(3))
    .Build();
```
<!-- endSnippet -->

See [fallback after retries](strategies/fallback.md#fallback-after-retries) for an example on how the strategies are executed.

> [!TIP]
>
> Things to remember:
>
> - Use `ResiliencePipelineBuilder{<TResult>}` to build a resiliency pipeline
> - Use multiple `Add*` builder methods to add new strategies to your pipeline
>
> For further information please check out the [Resilience pipelines documentation](pipelines/index.md).

## Migrating retry policies

This section describes how to migrate v7 retry policies to V8 retry strategies.

### Retry in v7

In v7 the retry policy is configured as:

<!-- snippet: migration-retry-v7 -->
```cs
// Retry once
Policy
    .Handle<SomeExceptionType>()
    .Retry();

// Retry multiple times
Policy
    .Handle<SomeExceptionType>()
    .Retry(3);

// Retry multiple times with callback
Policy
    .Handle<SomeExceptionType>()
    .Retry(3, onRetry: (exception, retryCount) =>
    {
        // Add logic to be executed before each retry, such as logging
    });

// Retry forever
Policy
    .Handle<SomeExceptionType>()
    .RetryForever();
```
<!-- endSnippet -->

### Retry in v8

In v8 the retry strategy is configured as:

<!-- snippet: migration-retry-v8 -->
```cs
// Retry once
//
// Because we are adding retries to a non-generic pipeline,
// we use the non-generic RetryStrategyOptions.
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    // PredicateBuilder is used to simplify the initialization of predicates.
    // Its API should be familiar to the v7 way of configuring what exceptions to handle.
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    MaxRetryAttempts = 1,
    // To disable waiting between retries, set the Delay property to TimeSpan.Zero.
    Delay = TimeSpan.Zero,
})
.Build();

// Retry multiple times
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    MaxRetryAttempts = 3,
    Delay = TimeSpan.Zero,
})
.Build();

// Retry multiple times with callback
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    MaxRetryAttempts = 3,
    Delay = TimeSpan.Zero,
    OnRetry = static args =>
    {
        // Add logic to be executed before each retry, such as logging
        return default;
    }
})
.Build();

// Retry forever
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    // To retry forever, set the MaxRetryAttempts property to int.MaxValue.
    MaxRetryAttempts = int.MaxValue,
    Delay = TimeSpan.Zero,
})
.Build();
```
<!-- endSnippet -->

### Retry and wait in v7

<!-- snippet: migration-retry-wait-v7 -->
```cs
// Wait and retry multiple times
Policy
    .Handle<SomeExceptionType>()
    .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));

// Wait and retry multiple times with callback
Policy
    .Handle<SomeExceptionType>()
    .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1), onRetry: (exception, retryCount) =>
    {
        // Add logic to be executed before each retry, such as logging
    });

// Wait and retry forever
Policy
    .Handle<SomeExceptionType>()
    .WaitAndRetryForever(_ => TimeSpan.FromSeconds(1));
```
<!-- endSnippet -->

### Retry and wait in v8

<!-- snippet: migration-retry-wait-v8 -->
```cs
// Wait and retry multiple times
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    MaxRetryAttempts = 3,
    Delay = TimeSpan.FromSeconds(1),
    BackoffType = DelayBackoffType.Constant
})
.Build();

// Wait and retry multiple times with callback
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    MaxRetryAttempts = 3,
    Delay = TimeSpan.FromSeconds(1),
    BackoffType = DelayBackoffType.Constant,
    OnRetry = static args =>
    {
        // Add logic to be executed before each retry, such as logging
        return default;
    }
})
.Build();

// Wait and retry forever
new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
    MaxRetryAttempts = int.MaxValue,
    Delay = TimeSpan.FromSeconds(1),
    BackoffType = DelayBackoffType.Constant
})
.Build();
```
<!-- endSnippet -->

### Retry results in v7

<!-- snippet: migration-retry-reactive-v7 -->
```cs
// Wait and retry with result handling
Policy
    .Handle<SomeExceptionType>()
    .OrResult<HttpResponseMessage>(result => result.StatusCode == HttpStatusCode.InternalServerError)
    .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));
```
<!-- endSnippet -->

### Retry results in v8

<!-- snippet: migration-retry-reactive-v8 -->
```cs
// Shows how to add a retry strategy that also retries particular results.
new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
{
    // PredicateBuilder is a convenience API that can used to configure the ShouldHandle predicate.
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<SomeExceptionType>()
        .HandleResult(static result => result.StatusCode == HttpStatusCode.InternalServerError),
    MaxRetryAttempts = 3,
})
.Build();

// The same as above, but using the switch expressions for best performance.
new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
{
    // Determine what results to retry using switch expressions.
    // Note that PredicateResult.True() is just a shortcut for "new ValueTask<bool>(true)".
    ShouldHandle = static args => args.Outcome switch
    {
        { Exception: SomeExceptionType } => PredicateResult.True(),
        { Result: { StatusCode: HttpStatusCode.InternalServerError } } => PredicateResult.True(),
        _ => PredicateResult.False()
    },
    MaxRetryAttempts = 3,
})
.Build();
```
<!-- endSnippet -->

> [!TIP]
>
> Things to remember:
>
> - Use `AddRetry` to add a retry strategy to your resiliency pipeline
> - Use the `RetryStrategyOptions{<TResult>}` to customize your retry behavior to meet your requirements
>
> For further information please check out the [Retry resilience strategy documentation](strategies/retry.md).

## Migrating rate limit policies

The rate limit policy is now replaced by the [rate limiter strategy](strategies/rate-limiter.md) which uses the [`System.Threading.RateLimiting`](https://www.nuget.org/packages/System.Threading.RateLimiting) package. Polly does not implement its own rate limiter anymore.

### Rate limit in v7

<!-- snippet: migration-rate-limit-v7 -->
```cs
// Create sync rate limiter
ISyncPolicy syncPolicy = Policy.RateLimit(
    numberOfExecutions: 100,
    perTimeSpan: TimeSpan.FromMinutes(1));

// Create async rate limiter
IAsyncPolicy asyncPolicy = Policy.RateLimitAsync(
    numberOfExecutions: 100,
    perTimeSpan: TimeSpan.FromMinutes(1));

// Create generic sync rate limiter
ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy.RateLimit<HttpResponseMessage>(
    numberOfExecutions: 100,
    perTimeSpan: TimeSpan.FromMinutes(1));

// Create generic async rate limiter
IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy.RateLimitAsync<HttpResponseMessage>(
    numberOfExecutions: 100,
    perTimeSpan: TimeSpan.FromMinutes(1));
```
<!-- endSnippet -->

### Rate limit in v8

> [!NOTE]
> In v8, you have to add the [`Polly.RateLimiting`](https://www.nuget.org/packages/Polly.RateLimiting) package to your application otherwise you won't see the `AddRateLimiter` extension.

<!-- snippet: migration-rate-limit-v8 -->
```cs
// The equivalent to Polly v7's RateLimit is the SlidingWindowRateLimiter.
//
// Polly exposes just a simple wrapper to the APIs exposed by the System.Threading.RateLimiting APIs.
// There is no need to create separate instances for sync and async flows as ResiliencePipeline handles both scenarios.
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
    {
        PermitLimit = 100,
        SegmentsPerWindow = 4,
        Window = TimeSpan.FromMinutes(1),
    }))
    .Build();

// The creation of generic pipeline is almost identical.
//
// Polly exposes the same set of rate-limiter extensions for both ResiliencePipeline<HttpResponseMessage> and ResiliencePipeline.
ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
    {
        PermitLimit = 100,
        SegmentsPerWindow = 4,
        Window = TimeSpan.FromMinutes(1),
    }))
    .Build();
```
<!-- endSnippet -->

> [!TIP]
>
> Things to remember:
>
> - Use `AddRateLimiter` to add a rate limiter strategy to your resiliency pipeline
> - Use one of the derived classes of [`ReplenishingRateLimiter`](https://learn.microsoft.com/dotnet/api/system.threading.ratelimiting.replenishingratelimiter) to customize your rate limiter behavior to meet your requirements
>
> For further information please check out the [Rate limiter resilience strategy documentation](strategies/rate-limiter.md).

## Migrating bulkhead policies

The bulkhead policy is now replaced by the [rate limiter strategy](strategies/rate-limiter.md) which uses the [`System.Threading.RateLimiting`](https://www.nuget.org/packages/System.Threading.RateLimiting) package. The new counterpart to bulkhead is `ConcurrencyLimiter`.

> [!NOTE]
> In v7, the bulkhead was presented as an individual strategy. In v8, it's not separately exposed because it's essentially a specialized type of rate limiter: the `ConcurrencyLimiter`.

### Bulkhead in v7

<!-- snippet: migration-bulkhead-v7 -->
```cs
// Create sync bulkhead
ISyncPolicy syncPolicy = Policy.Bulkhead(
    maxParallelization: 100,
    maxQueuingActions: 50);

// Create async bulkhead
IAsyncPolicy asyncPolicy = Policy.BulkheadAsync(
    maxParallelization: 100,
    maxQueuingActions: 50);

// Create generic sync bulkhead
ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy.Bulkhead<HttpResponseMessage>(
    maxParallelization: 100,
    maxQueuingActions: 50);

// Create generic async bulkhead
IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy.BulkheadAsync<HttpResponseMessage>(
    maxParallelization: 100,
    maxQueuingActions: 50);
```
<!-- endSnippet -->

### Bulkhead in v8

> [!NOTE]
> In v8, you have to add the [`Polly.RateLimiting`](https://www.nuget.org/packages/Polly.RateLimiting) package to your application otherwise you won't see the `AddConcurrencyLimiter` extension.

<!-- snippet: migration-bulkhead-v8 -->
```cs
// Create pipeline with concurrency limiter. Because ResiliencePipeline supports both sync and async
// callbacks, there is no need to define it twice.
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddConcurrencyLimiter(permitLimit: 100, queueLimit: 50)
    .Build();

// Create a generic pipeline with concurrency limiter. Because ResiliencePipeline<T> supports both sync and async
// callbacks, there is no need to define it twice.
ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddConcurrencyLimiter(permitLimit: 100, queueLimit: 50)
    .Build();
```
<!-- endSnippet -->

> [!TIP]
>
> Things to remember:
>
> - Use `AddConcurrencyLimiter` to add a concurrency limiter strategy to your resiliency pipeline
> - Use the `ConcurrencyLimiterOptions` to customize your concurrency limiter behavior to meet your requirements
>
> For further information please check out the [Rate limiter resilience strategy documentation](strategies/rate-limiter.md).

## Migrating timeout policies

> [!NOTE]
> In v8, the timeout resilience strategy does not support pessimistic timeouts because they can cause thread-pool starvation and non-cancellable background tasks. To address this, you can use [this workaround](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#cancelling-uncancellable-operations) to make the action cancellable.

### Timeout in v7

<!-- snippet: migration-timeout-v7 -->
```cs
// Create sync timeout
ISyncPolicy syncPolicy = Policy.Timeout(TimeSpan.FromSeconds(10));

// Create async timeout
IAsyncPolicy asyncPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10));

// Create generic sync timeout
ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy.Timeout<HttpResponseMessage>(TimeSpan.FromSeconds(10));

// Create generic async timeout
IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
```
<!-- endSnippet -->

### Timeout in v8

<!-- snippet: migration-timeout-v8 -->
```cs
// Create pipeline with timeout. Because ResiliencePipeline supports both sync and async
// callbacks, there is no need to define it twice.
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddTimeout(TimeSpan.FromSeconds(10))
    .Build();

// Create a generic pipeline with timeout. Because ResiliencePipeline<T> supports both sync and async
// callbacks, there is no need to define it twice.
ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddTimeout(TimeSpan.FromSeconds(10))
    .Build();
```
<!-- endSnippet -->

> [!TIP]
>
> Things to remember:
>
> - Use `AddTimeout` to add a timeout strategy to your resiliency pipeline
> - Use the `TimeoutStrategyOptions` to customize your timeout behavior to meet your requirements
>
> For further information please check out the [Timeout resilience strategy documentation](strategies/timeout.md).

## Migrating circuit breaker policies

This section describes how to migrate v7 circuit breaker policies to V8 circuit breaker strategies.

### Circuit breaker in v7

V7's "Standard" Circuit Breaker policy could be defined like below:

<!-- snippet: migration-circuit-breaker-v7 -->
```cs
// Create sync circuit breaker
ISyncPolicy syncPolicy = Policy
    .Handle<SomeExceptionType>()
    .CircuitBreaker(
        exceptionsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(1));

// Create async circuit breaker
IAsyncPolicy asyncPolicy = Policy
    .Handle<SomeExceptionType>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(1));

// Create generic sync circuit breaker
ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy<HttpResponseMessage>
    .Handle<SomeExceptionType>()
    .CircuitBreaker(
        handledEventsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(1));

// Create generic async circuit breaker
IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy<HttpResponseMessage>
    .Handle<SomeExceptionType>()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(1));
```
<!-- endSnippet -->

V7's Advanced Circuit Breaker policy could be defined like below:

<!-- snippet: migration-advanced-circuit-breaker-v7 -->
```cs
// Create sync advanced circuit breaker
ISyncPolicy syncPolicy = Policy
    .Handle<SomeExceptionType>()
    .AdvancedCircuitBreaker(
        failureThreshold: 0.5d,
        samplingDuration: TimeSpan.FromSeconds(5),
        minimumThroughput: 2,
        durationOfBreak: TimeSpan.FromSeconds(1));

// Create async advanced circuit breaker
IAsyncPolicy asyncPolicy = Policy
    .Handle<SomeExceptionType>()
    .AdvancedCircuitBreakerAsync(
        failureThreshold: 0.5d,
        samplingDuration: TimeSpan.FromSeconds(5),
        minimumThroughput: 2,
        durationOfBreak: TimeSpan.FromSeconds(1));

// Create generic sync advanced circuit breaker
ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy<HttpResponseMessage>
    .Handle<SomeExceptionType>()
    .AdvancedCircuitBreaker(
        failureThreshold: 0.5d,
        samplingDuration: TimeSpan.FromSeconds(5),
        minimumThroughput: 2,
        durationOfBreak: TimeSpan.FromSeconds(1));

// Create generic async advanced circuit breaker
IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy<HttpResponseMessage>
    .Handle<SomeExceptionType>()
    .AdvancedCircuitBreakerAsync(
        failureThreshold: 0.5d,
        samplingDuration: TimeSpan.FromSeconds(5),
        minimumThroughput: 2,
        durationOfBreak: TimeSpan.FromSeconds(1));

// Check circuit state
ICircuitBreakerPolicy cbPolicy = (ICircuitBreakerPolicy)asyncPolicy;
bool isOpen = cbPolicy.CircuitState == CircuitState.Open || cbPolicy.CircuitState == CircuitState.Isolated;

// Manually control state
cbPolicy.Isolate(); // Transitions into the Isolated state
cbPolicy.Reset(); // Transitions into the Closed state
```
<!-- endSnippet -->

### Circuit breaker in v8

> [!NOTE]
>
> Polly V8 does not support the standard (*"classic"*) circuit breaker with consecutive failure counting.
>
> In case of V8 you can define a Circuit Breaker strategy which works like the advanced circuit breaker in V7.

<!-- snippet: migration-circuit-breaker-v8 -->
```cs
// Create pipeline with circuit breaker. Because ResiliencePipeline supports both sync and async
// callbacks, there is no need to define it twice.
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
        FailureRatio = 0.5d,
        SamplingDuration = TimeSpan.FromSeconds(5),
        MinimumThroughput = 2,
        BreakDuration = TimeSpan.FromSeconds(1)
    })
    .Build();

// Create a generic pipeline with circuit breaker. Because ResiliencePipeline<T> supports both sync and async
// callbacks, there is also no need to define it twice.
ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<SomeExceptionType>(),
        FailureRatio = 0.5d,
        SamplingDuration = TimeSpan.FromSeconds(5),
        MinimumThroughput = 2,
        BreakDuration = TimeSpan.FromSeconds(1)
    })
    .Build();

// Check circuit state
CircuitBreakerStateProvider stateProvider = new();
// Manually control state
CircuitBreakerManualControl manualControl = new();

ResiliencePipeline pipelineState = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
        FailureRatio = 0.5d,
        SamplingDuration = TimeSpan.FromSeconds(5),
        MinimumThroughput = 2,
        BreakDuration = TimeSpan.FromSeconds(1),
        StateProvider = stateProvider,
        ManualControl = manualControl
    })
    .Build();

// Check circuit state
bool isOpen = stateProvider.CircuitState == CircuitState.Open || stateProvider.CircuitState == CircuitState.Isolated;

// Manually control state
await manualControl.IsolateAsync(); // Transitions into the Isolated state
await manualControl.CloseAsync(); // Transitions into the Closed state
```
<!-- endSnippet -->

> [!NOTE]
>
> In case of V7 you could do an optimization to reduce the thrown exceptions.
>
> You could guard the `Execute{Async}` call with a condition that the circuit is not broken. This technique does **not** work with V8.
>
> Under the [circuit breaker's anti-patterns](strategies/circuit-breaker.md#reducing-thrown-exceptions) you can find the suggested way for V8.

____

> [!TIP]
>
> Things to remember:
>
> - Use `AddCircuitBreaker` to add a circuit breaker strategy to your resiliency pipeline
> - Use the `CircuitBreakerStrategyOptions{<TResult>}` to customize your circuit breaker behavior to meet your requirements
>
> For further information please check out the [Circuit Breaker resilience strategy documentation](strategies/circuit-breaker.md).

## Migrating `Polly.Context`

The successor of the `Polly.Context` is the `ResilienceContext`. The major differences:

- `ResilienceContext` is pooled for enhanced performance and cannot be directly created. Instead, use the `ResilienceContextPool` class to get an instance.
- `Context` allowed directly custom data attachment, whereas `ResilienceContext` employs the `ResilienceContext.Properties` for the same purpose.
- In order to set or get a custom data you need to utilize the generic `ResiliencePropertyKey` structure.

### Predefined keys

| In V7 | In V8 |
| :-- | :-- |
| `OperationKey` | It can be used in the same way |
| `PolicyKey` | It's been relocated to `ResiliencePipelineBuilder` and used for [telemetry](advanced/telemetry.md#metrics) |
| `PolicyWrapKey` | It's been relocated to `ResiliencePipelineBuilder` and used for [telemetry](advanced/telemetry.md#metrics) |
| `CorrelationId` | It's been removed. For similar functionality, you can either use `System.Diagnostics.Activity.Current.Id` or attach your custom Id using `ResilienceContext.Properties`. |

- Additionally, `ResilienceContext` introduces a new property for `CancellationToken`.

### `Context` in v7

<!-- snippet: migration-context-v7 -->
```cs
// Create context
Context context = new Context();

// Create context with operation key
context = new Context("my-operation-key");

// Attach custom properties
context[Key1] = "value-1";
context[Key2] = 100;

// Retrieve custom properties
string value1 = (string)context[Key1];
int value2 = (int)context[Key2];

// Bulk attach
context = new Context("my-operation-key", new Dictionary<string, object>
{
    { Key1 , "value-1" },
    { Key2 , 100 }
});
```
<!-- endSnippet -->

### `ResilienceContext` in v8

<!-- snippet: migration-context-v8 -->
```cs
// Create context
ResilienceContext context = ResilienceContextPool.Shared.Get();

// Create context with operation key
context = ResilienceContextPool.Shared.Get("my-operation-key");

// Attach custom properties
ResiliencePropertyKey<string> propertyKey1 = new(Key1);
context.Properties.Set(propertyKey1, "value-1");

ResiliencePropertyKey<int> propertyKey2 = new(Key2);
context.Properties.Set(propertyKey2, 100);

// Bulk attach
context.Properties.SetProperties(new Dictionary<string, object?>
{
    { Key1 , "value-1" },
    { Key2 , 100 }
}, out var oldProperties);

// Retrieve custom properties
string value1 = context.Properties.GetValue(propertyKey1, "default");
int value2 = context.Properties.GetValue(propertyKey2, 0);

// Return the context to the pool
ResilienceContextPool.Shared.Return(context);
```
<!-- endSnippet -->

> [!TIP]
>
> Things to remember:
>
> - Use `ResilienceContextPool.Shared` to get a context and return it back to the pool
> - Use the `ResiliencePropertyKey<TValue>` to define type-safe keys for your custom data
>
> For further information please check out the [Resilience Context documentation](advanced/resilience-context.md).

## Migrating safe execution

In v7, the `ExecuteAndCapture{Async}` methods are considered the safe counterpart of the `Execute{Async}`.

The former does not throw an exception in case of failure rather than wrap the outcome in a result object.

In v8, the `ExecuteOutcomeAsync` method should be used to execute the to-be-decorated method in a safe way.

### `ExecuteAndCapture{Async}` in V7

<!-- snippet: migration-execute-v7 -->
```cs
// Synchronous execution
ISyncPolicy<int> syncPolicy = Policy.Timeout<int>(TimeSpan.FromSeconds(1));
PolicyResult<int> policyResult = syncPolicy.ExecuteAndCapture(Method);

// Asynchronous execution
IAsyncPolicy<int> asyncPolicy = Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(1));
PolicyResult<int> asyncPolicyResult = await asyncPolicy.ExecuteAndCaptureAsync(MethodAsync, CancellationToken.None);

// Assess policy result
if (policyResult.Outcome == OutcomeType.Successful)
{
    int result = policyResult.Result;

    // Process result
}
else
{
    Exception exception = policyResult.FinalException;
    FaultType faultType = policyResult.FaultType!.Value;
    ExceptionType exceptionType = policyResult.ExceptionType!.Value;

    // Process failure
}

// Access context
const string Key = "context_key";
IAsyncPolicy<int> asyncPolicyWithContext = Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(10),
    onTimeoutAsync: (ctx, ts, task) =>
    {
        ctx[Key] = "context_value";
        return Task.CompletedTask;
    });

asyncPolicyResult = await asyncPolicyWithContext.ExecuteAndCaptureAsync((ctx, token) => MethodAsync(token), new Context(), CancellationToken.None);
string? ctxValue = asyncPolicyResult.Context.GetValueOrDefault(Key) as string;
```
<!-- endSnippet -->

### `ExecuteOutcomeAsync` in V8

> [!NOTE]
>
> Polly V8 does not provide an API to synchronously execute and capture the outcome of a pipeline.

<!-- snippet: migration-execute-v8 -->
```cs
ResiliencePipeline<int> pipeline = new ResiliencePipelineBuilder<int>()
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();

// Synchronous execution
// Polly v8 does not support

// Asynchronous execution
var context = ResilienceContextPool.Shared.Get();
Outcome<int> pipelineResult = await pipeline.ExecuteOutcomeAsync(
    static async (ctx, state) => Outcome.FromResult(await MethodAsync(ctx.CancellationToken)), context, "state");
ResilienceContextPool.Shared.Return(context);

// Assess policy result
if (pipelineResult.Exception is null)
{
    int result = pipelineResult.Result;

    // Process result
}
else
{
    Exception exception = pipelineResult.Exception;

    // Process failure

    // If needed you can rethrow the exception
    pipelineResult.ThrowIfException();
}

// Access context
ResiliencePropertyKey<string> contextKey = new("context_key");
ResiliencePipeline<int> pipelineWithContext = new ResiliencePipelineBuilder<int>()
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(1),
        OnTimeout = args =>
        {
            args.Context.Properties.Set(contextKey, "context_value");
            return default;
        }
    })
    .Build();

context = ResilienceContextPool.Shared.Get();
pipelineResult = await pipelineWithContext.ExecuteOutcomeAsync(
    static async (ctx, state) => Outcome.FromResult(await MethodAsync(ctx.CancellationToken)), context, "state");

context.Properties.TryGetValue(contextKey, out var ctxValue);
ResilienceContextPool.Shared.Return(context);
```
<!-- endSnippet -->

> [!TIP]
>
> Things to remember:
>
> - Use `ExecuteOutcomeAsync` to execute your callback in a safe way

## Migrating no-op policies

| In V7 | In V8 |
| :-- | :-- |
| `Policy.NoOp` | `ResiliencePipeline.Empty` |
| `Policy.NoOpAsync` | `ResiliencePipeline.Empty` |
| `Policy.NoOp<TResult>` | `ResiliencePipeline<TResult>.Empty` |
| `Policy.NoOpAsync<TResult>` | `ResiliencePipeline<TResult>.Empty` |

## Migrating policy registries

In v7, the following registry APIs are exposed:

- `IConcurrentPolicyRegistry<TKey>`
- `IPolicyRegistry<TKey>`
- `IReadOnlyPolicyRegistry<TKey>`
- `PolicyRegistry<TKey>`

In v8, these have been replaced by:

- `ResiliencePipelineRegistry<TKey>`: Allows adding and accessing resilience pipelines.
- `ResiliencePipelineProvider<TKey>`: Read-only access to resilience pipelines.

The main updates:

- It's **append-only**, which means removal of items is not supported to avoid race conditions.
- It's thread-safe and supports features like dynamic reloading and resource disposal.
- It allows dynamic creation and caching of resilience pipelines using pre-registered delegates.
- Type safety is enhanced, eliminating the need for casting between policy types.

### Registry in v7

<!-- snippet: migration-registry-v7 -->
```cs
// Create a registry
var registry = new PolicyRegistry();

// Add a policy
registry.Add(PolicyKey, Policy.Timeout(TimeSpan.FromSeconds(10)));

// Try get a policy
registry.TryGet<IAsyncPolicy>(PolicyKey, out IAsyncPolicy? policy);

// Try get a generic policy
registry.TryGet<IAsyncPolicy<string>>(PolicyKey, out IAsyncPolicy<string>? genericPolicy);

// Update a policy
registry.AddOrUpdate(
    PolicyKey,
    Policy.Timeout(TimeSpan.FromSeconds(10)),
    (key, previous) => Policy.Timeout(TimeSpan.FromSeconds(10)));
```
<!-- endSnippet -->

### Registry in v8

> [!NOTE]
>
> Polly V8 does not provide an explicit API to directly update a strategy in the registry.
>
> On the other hand it does provide a mechanism to [reload pipelines](pipelines/resilience-pipeline-registry.md#dynamic-reloads).

<!-- snippet: migration-registry-v8 -->
```cs
// Create a registry
var registry = new ResiliencePipelineRegistry<string>();

// Add a pipeline using a builder, when the pipeline is retrieved it will be dynamically built and cached
registry.TryAddBuilder(PipelineKey, (builder, context) => builder.AddTimeout(TimeSpan.FromSeconds(10)));

// Try get a pipeline
registry.TryGetPipeline(PipelineKey, out ResiliencePipeline? pipeline);

// Try get a generic pipeline
registry.TryGetPipeline<string>(PipelineKey, out ResiliencePipeline<string>? genericPipeline);

// Get or add pipeline
registry.GetOrAddPipeline(PipelineKey, builder => builder.AddTimeout(TimeSpan.FromSeconds(10)));
```
<!-- endSnippet -->

> [!TIP]
>
> Things to remember:
>
> - Use `ResiliencePipelineRegistry<TResult>` to add or get a pipelines to the registry
> - Prefer the safer methods (for example: `TryGetPipeline{<TResult>}`) over their counterpart (for example: `GetPipeline{<TResult>}`)
>
> For further information please check out the [Resilience pipeline registry documentation](pipelines/resilience-pipeline-registry.md).

## Interoperability between policies and resilience pipelines

In certain scenarios, you might not able to migrate all your code to the v8 API.

In the name of interoperability you can define V8 strategies use them with your v7 policies.

V8 provides a set of extension methods to support easy conversion from v8 to v7 APIs, as shown in the example below:

> [!NOTE]
> In v8, you have to add the [`Polly.RateLimiting`](https://www.nuget.org/packages/Polly.RateLimiting) package to your application otherwise you won't see the `AddRateLimiter` extension.

<!-- snippet: migration-interoperability -->
```cs
// First, create a resilience pipeline.
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRateLimiter(new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
    {
        Window = TimeSpan.FromSeconds(10),
        PermitLimit = 100
    }))
    .Build();

// Now, convert it to a v7 policy. Note that it can be converted to both sync and async policies.
ISyncPolicy syncPolicy = pipeline.AsSyncPolicy();
IAsyncPolicy asyncPolicy = pipeline.AsAsyncPolicy();

// Finally, use it in a policy wrap.
ISyncPolicy wrappedPolicy = Policy.Wrap(
    syncPolicy,
    Policy.Handle<SomeExceptionType>().Retry(3));
```
<!-- endSnippet -->
