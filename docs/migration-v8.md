# Migration guide from v7 to v8

Welcome to the migration guide for Polly's v8 version. The v8 version of Polly brings major new enhancements and supports all the same scenarios as previous versions. In the following sections, we'll detail the differences between the v7 and v8 APIs and provide steps on how to transition smoothly.

> [!NOTE]
> The v7 API is still available and fully supported even when using the v8 version by referencing [Polly](https://www.nuget.org/packages/Polly) package.

## Major differences

- **The term *Policy* is now replaced with *Strategy***: In previous versions, Polly used the term *policy* for retries, timeouts, etc. In v8, these are referred to as *resilience strategies*.
- **Introduction of Resilience Pipelines**: The [resilience pipeline](pipelines/index.md) combines one or more resilience strategies. This is the foundational API for Polly v8, similar to the **Policy Wrap** in previous versions but integrated into the core API.
- **Unified sync and async flows**: Interfaces such as `IAsyncPolicy`, `IAsyncPolicy<T>`, `ISyncPolicy`, `ISyncPolicy<T>`, and `IPolicy` are now unified under `ResiliencePipeline` and `ResiliencePipeline<T>`. The resilience pipeline supports both synchronous and asynchronous execution flows.
- **Native async support**: Polly v8 was designed with asynchronous support from the start.
- **No static APIs**: Unlike previous versions, v8 doesn't use static APIs. This improves testability and extensibility while maintaining ease of use.
- **Options-based configuration**: Configuring individual resilience strategies is now options-based, offering more flexibility and improving maintainability and extensibility.
- **Built-in telemetry**: Polly v8 now has built-in telemetry support.
- **Improved performance and low-allocation APIs**: Polly v8 boasts significant performance enhancements and provides zero-allocation APIs for advanced use cases.

> [!NOTE]
> Please read the comments in the code carefully for additional context and explanations.

## Migrating execution policies

This section describes how to migrate from execution policies (i.e. `IAsyncPolicy`, `ISyncPolicy`) to resilience pipelines (i.e. `ResiliencePipeline`, `ResiliencePipeline<T>`).

### Configuring policies in v7

In earlier versions, Polly exposed various interfaces to execute user code:

- `IAsyncPolicy`
- `IAsyncPolicy<T>`
- `ISyncPolicy`
- `ISyncPolicy<T>`

These interfaces were created and used as:

<!-- snippet: migration-policies-v7 -->
```cs
// Create and use the ISyncPolicy.
ISyncPolicy syncPolicy = Policy.Handle<Exception>().WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));
syncPolicy.Execute(() =>
{
    // your code here
});

// Create and use the IAsyncPolicy
IAsyncPolicy asyncPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));
await asyncPolicy.ExecuteAsync(
    async cancellationToken =>
    {
        // your code here
    },
    cancellationToken);

// Create and use the ISyncPolicy<T>
ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));

syncPolicyT.Execute(() =>
{
    // your code here
    return GetResponse();
});

// Create and use the IAsyncPolicy<T>
IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));
await asyncPolicyT.ExecuteAsync(
    async cancellationToken =>
    {
        // your code here
        return await GetResponseAsync(cancellationToken);
    },
    cancellationToken);
```
<!-- endSnippet -->

### Configuring strategies in v8

In Polly v8, the previous code becomes:

<!-- snippet: migration-policies-v8 -->
```cs
// Create and use the ResiliencePipeline.
//
// The ResiliencePipelineBuilder is used to start building the resilience pipeline,
// instead of the static Policy.HandleException<TException>() call.
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        Delay = TimeSpan.FromSeconds(1),
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Constant
    })
    .Build(); // After all necessary strategies are added, call build to create the pipeline.

// Synchronous execution
pipeline.Execute(() =>
{
    // your code here
});

// Asynchronous execution is also supported with the same pipeline instance
await pipeline.ExecuteAsync(static async cancellationToken =>
{
    // your code here
},
cancellationToken);

// Create and use the ResiliencePipeline<T>.
//
// Building of generic resilience pipeline is very similar to non-generic one.
// Notice the use of generic RetryStrategyOptions<HttpResponseMessage> to configure the strategy
// As opposed to providing the arguments into the method.
ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<Exception>()
            .HandleResult(result => !result.IsSuccessStatusCode),
        Delay = TimeSpan.FromSeconds(1),
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Constant
    })
    .Build();

// Synchronous execution
pipelineT.Execute(() =>
{
    // your code here
    return GetResponse();
});

// Asynchronous execution
await pipelineT.ExecuteAsync(static async cancellationToken =>
{
    // your code here
    return await GetResponseAsync(cancellationToken);
},
cancellationToken);
```
<!-- endSnippet -->

## Migrating policy wrap

### Policy wrap in v7

Policy wrap is used to combine multiple policies into one as shown in the v7 example below:

<!-- snippet: migration-policy-wrap-v7 -->
```cs
IAsyncPolicy retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));

IAsyncPolicy timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(3));

// Wrap the policies. Tne policies are executed in the following order:
// 1. Retry
// 2. Timeout
IAsyncPolicy wrappedPolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy);
```
<!-- endSnippet -->

### Policy wrap in v8

In v8, there's no need to use policy wrap explicitly. Instead, policy wrapping is smoothly integrated into `ResiliencePipelineBuilder`, as shown in the example below:

<!-- snippet: migration-policy-wrap-v8 -->
```cs
// The "PolicyWrap" is integrated directly. Strategies are executed in the same order as they were added:
// 1. Retry
// 2. Timeout
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

> [!IMPORTANT]
> In v7, the policy wrap ordering is different; the policy added first was executed last. In v8, the execution order matches the order in which they were added.

## Migrating retry policy

This section describes how to migrate the v7 retry policy to a resilience strategy in v8.

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
    // Its API should be familiar to v7 way of configuring what exceptions to handle.
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
    OnRetry = args =>
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
// Retry forever
Policy
    .Handle<SomeExceptionType>()
    .WaitAndRetryForever(_ => TimeSpan.FromSeconds(1));

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
    OnRetry = args =>
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
  .OrResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.InternalServerError)
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
        .HandleResult(result => result.StatusCode == HttpStatusCode.InternalServerError),
    MaxRetryAttempts = 3,
})
.Build();

// The same as above, but using the switch expressions for max performance.
new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
{
    // Determine what results to retry using switch expressions.
    // Note that PredicateResult.True() is just a shortcut for "new ValueTask<bool>(true)".
    ShouldHandle = args => args.Outcome switch
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

It's important to remember that the configuration in v8 is options based, i.e. `RetryStrategyOptions` are used.

## Migrating rate limit policy

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
        Window = TimeSpan.FromMinutes(1),
    }))
    .Build();
```
<!-- endSnippet -->

## Migrating bulkhead policy

The bulkhead policy is now replaced by the [rate limiter strategy](strategies/rate-limiter.md) which uses the [`System.Threading.RateLimiting`](https://www.nuget.org/packages/System.Threading.RateLimiting) package. The new counterpart to bulkhead is `ConcurrencyLimiter`.

> [!NOTE]
> In v7, the bulkhead was presented as an individual strategy. In v8, it's not separately exposed because it's essentially a specialized type of rate limiter: the `ConcurrencyLimiter`.

### Bulkhead in v7

<!-- snippet: migration-bulkhead-v7 -->
```cs
// Create sync bulkhead
ISyncPolicy syncPolicy = Policy.Bulkhead(maxParallelization: 100, maxQueuingActions: 50);

// Create async bulkhead
IAsyncPolicy asyncPolicy = Policy.BulkheadAsync(maxParallelization: 100, maxQueuingActions: 50);

// Create generic sync bulkhead
ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy.Bulkhead<HttpResponseMessage>(maxParallelization: 100, maxQueuingActions: 50);

// Create generic async bulkhead
IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: 100, maxQueuingActions: 50);
```
<!-- endSnippet -->

### Bulkhead in v8

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

## Migrating timeout policy

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

## Migrating other policies

Migrating is a process similar to the one described in previous sections. Keep in mind:

- Strategy configurations (or policies in v7) are now in options. Property names should match the v7 APIs and scenarios.
- Use `ResiliencePipelineBuilder` or `ResiliencePipelineBuilder<T>` and their respective extensions to add specific strategies.
- For more details on each strategy, refer to the [resilience strategies](strategies/index.md) documentation.

## Migrating `Polly.Context`

`Polly.Context` has been succeeded by `ResilienceContext`. Here are the main changes:

- `ResilienceContext` is pooled for enhanced performance and isn't directly creatable. Instead, use the `ResilienceContextPool` to get an instance.
- Directly attaching custom data is supported by `Context`, whereas `ResilienceContext` employs the `ResilienceContext.Properties` property.
- Both `PolicyKey` and `PolicyWrapKey` are no longer a part of `ResilienceContext`. They've been relocated to `ResiliencePipelineBuilder` and are now used for [telemetry](advanced/telemetry.md#metrics).
- The `CorrelationId` property has been removed. For similar functionality, you can either use `System.Diagnostics.Activity.Current.Id` or attach your custom id using `ResilienceContext.Properties`.
- Additionally, `ResilienceContext` introduces the `CancellationToken` property.

### `Context` in v7

<!-- snippet: migration-context-v7 -->
```cs
// Create context
Context context = new Context();

// Create context with operation key
context = new Context("my-operation-key");

// Attach custom properties
context["prop-1"] = "value-1";
context["prop-2"] = 100;

// Retrieve custom properties
string value1 = (string)context["prop-1"];
int value2 = (int)context["prop-2"];
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
context.Properties.Set(new ResiliencePropertyKey<string>("prop-1"), "value-1");
context.Properties.Set(new ResiliencePropertyKey<int>("prop-2"), 100);

// Retrieve custom properties
string value1 = context.Properties.GetValue(new ResiliencePropertyKey<string>("prop-1"), "default");
int value2 = context.Properties.GetValue(new ResiliencePropertyKey<int>("prop-2"), 0);

// Return the context to the pool
ResilienceContextPool.Shared.Return(context);
```
<!-- endSnippet -->

For more details, refer to the [Resilience context](advanced/resilience-context.md) documentation.

## Migrating no-op policy

- For `Policy.NoOp` or `Policy.NoOpAsync`, switch to `ResiliencePipeline.Empty`.
- For `Policy.NoOp<T>` or `Policy.NoOpAsync<T>`, switch to `ResiliencePipeline<T>.Empty`.

## Migrating policy registry

In v7, the following registry APIs are exposed:

- `IPolicyRegistry<T>`
- `IReadOnlyPolicyRegistry<T>`
- `IConcurrentPolicyRegistry<T>`
- `PolicyRegistry<T>`

In v8, these have been replaced by:

- `ResiliencePipelineProvider<TKey>`: Allows adding and accessing resilience pipelines.
- `ResiliencePipelineRegistry<TKey>`: Read-only access to resilience pipelines.

The main updates in the new registry include:

- It's append-only, which means removal of items is not supported to avoid race conditions.
- It's thread-safe and supports features like dynamic reloads and resource disposal.
- It allows dynamic creation and caching of resilience pipelines (previously known as policies in v7) using pre-registered delegates.
- Type safety is enhanced, eliminating the need for casting between policy types.

For more details, refer to the [pipeline registry](pipelines/resilience-pipeline-registry.md) documentation.

### Registry in v7

<!-- snippet: migration-registry-v7 -->
```cs
// Create a registry
var registry = new PolicyRegistry();

// Try get a policy
registry.TryGet<IAsyncPolicy>("my-key", out IAsyncPolicy? policy);

// Try get a generic policy
registry.TryGet<IAsyncPolicy<string>>("my-key", out IAsyncPolicy<string>? genericPolicy);

// Add a policy
registry.Add("my-key", Policy.Timeout(TimeSpan.FromSeconds(10)));

// Update a policy
registry.AddOrUpdate(
    "my-key",
    Policy.Timeout(TimeSpan.FromSeconds(10)),
    (key, old) => Policy.Timeout(TimeSpan.FromSeconds(10)));
```
<!-- endSnippet -->

### Registry in v8

<!-- snippet: migration-registry-v8 -->
```cs
// Create a registry
var registry = new ResiliencePipelineRegistry<string>();

// Try get a pipeline
registry.TryGetPipeline("my-key", out ResiliencePipeline? pipeline);

// Try get a generic pipeline
registry.TryGetPipeline<string>("my-key", out ResiliencePipeline<string>? genericPipeline);

// Add a pipeline using a builder, when "my-key" pipeline is retrieved it will be dynamically built and cached
registry.TryAddBuilder("my-key", (builder, context) => builder.AddTimeout(TimeSpan.FromSeconds(10)));

// Get or add pipeline
registry.GetOrAddPipeline("my-key", builder => builder.AddTimeout(TimeSpan.FromSeconds(10)));
```
<!-- endSnippet -->

## Interoperability between policies and resilience pipelines

In certain scenarios, you might not want to migrate your code to v8. Instead, you may prefer to use strategies from v8 and apply them to v7 APIs. Polly provides a set of extensions to support easy conversion from v8 to v7 APIs, as shown in the example below:

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
