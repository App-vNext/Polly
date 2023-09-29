# Dependency injection

Starting with version 8, Polly provides features that make the integration of Polly
with the .NET [`IServiceCollection`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
Dependency Injection (DI) container more streamlined. This is a thin layer atop the
[resilience pipeline registry](../pipelines/resilience-pipeline-registry.md) which
manages resilience pipelines.

## Usage

To use the DI functionality, add the [`Polly.Extensions`](https://www.nuget.org/packages/Polly.Extensions)
package to your project:

```sh
dotnet add package Polly.Extensions
```

Afterwards, you can use the `AddResiliencePipeline(...)` extension method to set
up your pipeline:

<!-- snippet: add-resilience-pipeline -->
```cs
var services = new ServiceCollection();

// Define a resilience pipeline
services.AddResiliencePipeline("my-key", builder =>
{
    // Add strategies to your pipeline here, timeout for example
    builder.AddTimeout(TimeSpan.FromSeconds(10));
});

// You can also access IServiceProvider by using the alternate overload
services.AddResiliencePipeline("my-key", (builder, context) =>
{
    // Resolve any service from DI
    var loggerFactory = context.ServiceProvider.GetRequiredService<ILoggerFactory>();

    // Add strategies to your pipeline here
    builder.AddTimeout(TimeSpan.FromSeconds(10));
});

// Resolve the resilience pipeline
ServiceProvider serviceProvider = services.BuildServiceProvider();
ResiliencePipelineProvider<string> pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
ResiliencePipeline pipeline = pipelineProvider.GetPipeline("my-key");

// Use it
await pipeline.ExecuteAsync(
    static async cancellation => await Task.Delay(100, cancellation));
```
<!-- endSnippet -->

The `AddResiliencePipeline` extension method also registers the following services
into the DI container:

- `ResiliencePipelineRegistry<string>`: Allows adding and retrieving resilience pipelines.
- `ResiliencePipelineProvider<string>`: Allows retrieving resilience pipelines.
- `IOptions<ResiliencePipelineRegistryOptions<string>>`: Options for `ResiliencePipelineRegistry<string>`.

> [!NOTE]
> The generic `string` is inferred since the pipeline was defined using the
> "my-key" value.

If you only need the registry without defining a pipeline, use the
`AddResiliencePipelineRegistry(...)` method.

### Generic resilience pipelines

You can also define generic resilience pipelines (`ResiliencePipeline<T>`), as
demonstrated below:

<!-- snippet: add-resilience-pipeline-generic -->
```cs
var services = new ServiceCollection();

// Define a generic resilience pipeline
// First parameter is the type of key, second one is the type of the results the generic pipeline works with
services.AddResiliencePipeline<string, HttpResponseMessage>("my-pipeline", builder =>
{
    builder.AddRetry(new()
    {
        MaxRetryAttempts = 2,
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutRejectedException>()
            .HandleResult(response => response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
    })
    .AddTimeout(TimeSpan.FromSeconds(2));
});

// Resolve the resilience pipeline
ServiceProvider serviceProvider = services.BuildServiceProvider();
ResiliencePipelineProvider<string> pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
ResiliencePipeline<HttpResponseMessage> pipeline = pipelineProvider.GetPipeline<HttpResponseMessage>("my-key");

// Use it
await pipeline.ExecuteAsync(
    async cancellation => await client.GetAsync(endpoint, cancellation),
    cancellationToken);
```
<!-- endSnippet -->

## Dynamic reloads

Dynamic reloading is a feature of the pipeline registry that is also surfaced when
using the `AddResiliencePipeline(...)` extension method. Use an overload that provides
access to `AddResiliencePipelineContext`:

<!-- snippet: di-dynamic-reloads -->
```cs
services
    .Configure<RetryStrategyOptions>("my-retry-options", configurationSection) // Configure the options
    .AddResiliencePipeline("my-pipeline", (builder, context) =>
    {
        // Enable the reloads whenever the named options change
        context.EnableReloads<RetryStrategyOptions>("my-retry-options");

        // Utility method to retrieve the named options
        var retryOptions = context.GetOptions<RetryStrategyOptions>("my-retry-options");

        // Add retries using the resolved options
        builder.AddRetry(retryOptions);
    });
```
<!-- endSnippet -->

- `EnableReloads<T>(...)` activates the dynamic reloading of `my-pipeline`.
- `RetryStrategyOptions` are fetched using `context.GetOptions(...)` utility method.
- A retry strategy is added.

During a reload:

- The callback re-executes.
- The previous pipeline is discarded.

If an error occurs during reloading, the old pipeline remains, and dynamic
reloading stops.

## Resource disposal

Like dynamic reloading, the pipeline registry's resource disposal feature lets
you register callbacks. These callbacks run when the pipeline is discarded, reloaded,
or the registry is disposed at application shutdown.

See the example below:

<!-- snippet: di-resource-disposal -->
```cs
services.AddResiliencePipeline("my-pipeline", (builder, context) =>
{
    // Create disposable resource
    var limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions { PermitLimit = 100, QueueLimit = 100 });

    // Use it
    builder.AddRateLimiter(limiter);

    // Dispose the resource created in the callback when the pipeline is discarded
    context.OnPipelineDisposed(() => limiter.Dispose());
});
```
<!-- endSnippet -->

This feature ensures that resources are properly disposed when a pipeline
reloads, discarding the old version.

## Complex pipeline keys

The `AddResiliencePipeline(...)` method supports complex pipeline keys. This
capability allows you to define the structure of your pipeline and dynamically
resolve and cache multiple instances of the pipeline with different keys.

Start by defining your complex key:

<!-- snippet: di-registry-complex-key -->
```cs
public record struct MyPipelineKey(string PipelineName, string InstanceName)
{
}
```
<!-- endSnippet -->

Next, register your pipeline:

<!-- snippet: di-registry-add-pipeline -->
```cs
services.AddResiliencePipeline(new MyPipelineKey("my-pipeline", string.Empty), builder =>
{
    // Circuit breaker is a stateful strategy. To isolate the builder across different pipelines,
    // we must use multiple instances.
    builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions());
});
```
<!-- endSnippet -->

The "my-pipeline" pipeline is now registered. Note that the `InstanceName` is an
empty string. While we're registering the builder action for a specific pipeline,
the `InstanceName` parameter isn't used during the pipeline's registration. Some
further modifications are required for this to function.

Introduce the `PipelineNameComparer`:

<!-- snippet: di-complex-key-comparer -->
```cs
public sealed class PipelineNameComparer : IEqualityComparer<MyPipelineKey>
{
    public bool Equals(MyPipelineKey x, MyPipelineKey y) => x.PipelineName == y.PipelineName;

    public int GetHashCode(MyPipelineKey obj) => (obj.PipelineName, obj.InstanceName).GetHashCode();
}
```
<!-- endSnippet -->

Then, configure the registry behavior:

<!-- snippet: di-registry-configure -->
```cs
services
    .AddResiliencePipelineRegistry<MyPipelineKey>(options =>
    {
        options.BuilderComparer = new PipelineNameComparer();

        options.InstanceNameFormatter = key => key.InstanceName;

        options.BuilderNameFormatter = key => key.PipelineName;
    });
```
<!-- endSnippet -->

Let's summarize our actions:

- We assigned the `PipelineNameComparer` instance to the `BuilderComparer` property.
  This action changes the default registry behavior, ensuring that only the
  `PipelineName` is used to find the associated builder.
- We used the `InstanceNameFormatter` delegate to represent the `MyPipelineKey`
  as an instance name for telemetry purposes, keeping the instance name as it is.
- Likewise, the `BuilderNameFormatter` delegate represents the `MyPipelineKey` as
  a builder name in telemetry.

Finally, use the `ResiliencePipelineProvider<MyPipelineKey>` to dynamically create
and cache multiple instances of the same pipeline:

<!-- snippet: di-registry-multiple-instances -->
```cs
ResiliencePipelineProvider<MyPipelineKey> pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<MyPipelineKey>>();

// The registry dynamically creates and caches instance-A using the associated builder action
ResiliencePipeline instanceA = pipelineProvider.GetPipeline(new MyPipelineKey("my-pipeline", "instance-A"));

// The registry creates and caches instance-B
ResiliencePipeline instanceB = pipelineProvider.GetPipeline(new MyPipelineKey("my-pipeline", "instance-B"));
```
<!-- endSnippet -->

## Patterns and anti-patterns

Over the years, many developers have used Polly in various ways. Some of these
recurring patterns may not be ideal. This section highlights the recommended practices
and those to avoid.

### 1 - Accessing the `IServiceCollection` instead of `IServiceProvider`

❌ DON'T

Capture `IServiceCollection` inside `AddResiliencePipeline()`:

<!-- snippet: di-anti-pattern-1 -->
```cs
var services = new ServiceCollection();
services.AddResiliencePipeline("myFavoriteStrategy", builder =>
{
    builder.AddRetry(new()
    {
        OnRetry = args =>
        {
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger>();
            // ...
            return default;
        }
    });
});
```
<!-- endSnippet -->

**Reasoning**:

This approach builds a new `ServiceProvider` before each retry attempt _unnecessarily_.

✅ DO

Use another overload of `AddResiliencePipeline()` which allows access to `IServiceProvider`:

<!-- snippet: di-pattern-1 -->
```cs
var services = new ServiceCollection();
services.AddResiliencePipeline("myFavoriteStrategy", static (builder, context) =>
{
    builder.AddRetry(new()
    {
        OnRetry = args =>
        {
            var logger = context.ServiceProvider.GetService<ILogger>();
            // ...
            return default;
        }
    });
});
```
<!-- endSnippet -->

**Reasoning**:

This approach uses the already built `ServiceProvider` and uses the same instance
before every retry attempts.
