# Chaos engineering

**Chaos engineering** is the discipline of experimenting on a system in order to build confidence in the system's capability to withstand turbulent conditions in production.

If you want to learn more about chaos engineering:

- [Chaos engineering on Wikipedia](https://en.wikipedia.org/wiki/Chaos_engineering): Describes the basic concepts, history and tools related to chaos engineering.
- [Chaos engineering, the history, principles and practices](https://www.gremlin.com/community/tutorials/chaos-engineering-the-history-principles-and-practice/): Excellent article about chaos engineering by [Gremlin](https://www.gremlin.com/chaos-engineering/), a chaos engineering platform.
- [Understanding chaos engineering and resilience](https://learn.microsoft.com/azure/chaos-studio/chaos-studio-chaos-engineering-overview): Intro to chaos engineering in the context of [Azure Chaos Studio](https://learn.microsoft.com/en-us/azure/chaos-studio/chaos-studio-overview), managed service that uses chaos engineering to help you measure, understand, and improve your cloud application and service resilience.

## Chaos engineering with Simmy

[Simmy][simmy] is a major new addition to Polly library, adding a chaos engineering and fault-injection dimension to Polly, through the provision of strategies to selectively inject faults, latency, custom behavior or fake results.

![Simmy](../media/simmy-logo.png)

## Usage

<!-- snippet: chaos-usage -->
```cs
var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();

// First, configure regular resilience strategies
builder
    .AddConcurrencyLimiter(10, 100)
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage> { /* configure options */ })
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage> { /* configure options */ })
    .AddTimeout(TimeSpan.FromSeconds(5));

// Finally, configure chaos strategies if you want to inject chaos.
// These should come after the regular resilience strategies.

// 2% of total requests will be injected with chaos fault.
const double faultInjectionRate = 0.02;
// For the other 98% of total requests, 50% of them will be injected with latency. Then 49% of total request will be injected with chaos latency.
// Latency injection does not return early.
const double latencyInjectionRate = 0.50;
// For the other 98% of total requests, 10% of them will be injected with outcome. Then 9.8% of total request will be injected with chaos outcome.
const double outcomeInjectionRate = 0.10;
// For the other 89.2% of total requests, 1% of them will be injected with behavior. Then 0.892% of total request will be injected with chaos behavior.
const double behaviorInjectionRate = 0.01;

builder
    .AddChaosFault(faultInjectionRate, () => new InvalidOperationException("Injected by chaos strategy!")) // Inject a chaos fault to executions
    .AddChaosLatency(latencyInjectionRate, TimeSpan.FromMinutes(1)) // Inject a chaos latency to executions
    .AddChaosOutcome(outcomeInjectionRate, () => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)) // Inject a chaos outcome to executions
    .AddChaosBehavior(behaviorInjectionRate, cancellationToken => RestartRedisAsync(cancellationToken)); // Inject a chaos behavior to executions
```
<!-- endSnippet -->

> [!NOTE]
> It is usual to place the chaos strategy as the last strategy in the resilience pipeline. By placing the chaos strategies as last, they subvert the usual outbound call at the last minute, substituting their fault or adding extra latency, etc. The existing resilience strategies - further out in the `ResiliencePipeline` - still apply, so you can test how the Polly resilience strategies you have configured handle the chaos/faults injected by Simmy.

> [!NOTE]
> The `AddChaosFault`, `AddChaosLatency`, `AddChaosOutcome` will take effect sequentially if you combine them together. In the above example, we use `fault first then latency stragtegy`, it can save fault waiting time. If you put `AddChaosLatency` before `AddChaosFault`, you will get different behavior. 

## Major differences

This section highlights the major differences compared to the [`Polly.Contrib.Simmy`](https://github.com/Polly-Contrib/Simmy) library:

- **From `MonkeyPolicy` to `ChaosStrategy`**: We've updated the terminology from `Monkey` to `Chaos` to better align with the well-recognized principles of *chaos engineering*.
- **Unified configuration options**: The `InjectOptionsBase` and `InjectOptionsAsyncBase` are now consolidated into `ChaosStrategyOptions`. This change brings Simmy in line with the Polly v8 API, offering built-in support for options-based configuration and seamless integration of synchronous and asynchronous executions.
- **Chaos strategies enabled by default**: Adding a chaos strategy (previously known as monkey policy) now means it's active right away. This is a departure from earlier versions, where the monkey policy had to be explicitly enabled.
- **API changes**: The new version of Simmy introduces several API updates. While this list isn't complete, it includes key changes like renaming `Inject` to `AddChaos` and switching from `Result` to `Outcome`. Here are some specific renames:

| From              | To                 |
|-------------------|--------------------|
| `InjectException` | `AddChaosFault`    |
| `InjectResult`    | `AddChaosOutcome`  |
| `InjectBehavior`  | `AddChaosBehavior` |
| `InjectLatency`   | `AddChaosLatency`  |

- **Sync and async unification**: Before, Simmy had various methods to set policies like `InjectLatency`, `InjectLatencyAsync`, `InjectLatency<T>`, and `InjectLatencyAsync<T>`. With the new version based on Polly v8, these methods have been combined into a single `AddChaosLatency` extension that works for both `ResiliencePipelineBuilder` and `ResiliencePipelineBuilder<T>`. These rules are covering all types of chaos strategies (Outcome, Fault, Latency, and Behavior).

## Motivation

There are a lot of questions when it comes to chaos engineering and making sure that a system is actually ready to face the worst possible scenarios:

- Is my system resilient enough?
- Am I handling the right exceptions/scenarios?
- How will my system behave if X happens?
- How can I test without waiting for a handled (or even unhandled) exception to happen in my production environment?

Using Polly helps introduce resilience to a project, but we don't want to have to wait for expected or unexpected failures to test it out. A resilience could be wrongly implemented; testing the scenarios is not straightforward; and mocking failure of some dependencies (for example a cloud SaaS or PaaS service) is not always straightforward.

### What is needed to simulate chaotic scenarios?

- A way to simulate failures of dependencies (any service dependency for example).
- Define when to fail based on some external factors - maybe global configuration or some rule.
- A way to revert easily, to control the blast radius.
- To be production grade, to run this in a production or near-production system with automation.

## Chaos strategies

Chaos strategies (formerly known as Monkey strategies) are in essence a [Resilience strategy](../strategies/index.md#built-in-strategies), which means, as a *Resilience Strategy* is the minimum unit of resilience for Polly, a *Chaos Strategy* is the minimum unit of chaos for Simmy.

### Built-in strategies

| Strategy                | Type      | What does the strategy do?                                          |
|-------------------------|-----------|---------------------------------------------------------------------|
| [Fault](fault.md)       | Proactive | Injects exceptions in your system.                                  |
| [Outcome](outcome.md)   | Reactive  | Injects fake outcomes (results or exceptions) in your system.       |
| [Latency](latency.md)   | Proactive | Injects latency into executions before the calls are made.          |
| [Behavior](behavior.md) | Proactive | Allows you to inject *any* extra behavior, before a call is placed. |

## Common options across strategies

All the strategies' options implement the [`ChaosStrategyOptions`](xref:Polly.Simmy.ChaosStrategyOptions) class as it contains the basic configuration for every chaos strategy.

> [!IMPORTANT]
> Please bear in mind that with the V8 API the chaos strategies are enabled by default. So, you can opt-out of them one-by-one either via the `Enabled` or via the `EnabledGenerator` property.
>
> In previous Simmy versions you had to explicitly call either the `Enabled` or the `EnabledWhen` method to opt-in a chaos policy.

| Property                 | Default Value | Description                                                                                                                                                                                                                      |
|--------------------------|---------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `InjectionRate`          | 0.001         | A decimal between 0 and 1 inclusive. The strategy will inject the chaos, randomly, that proportion of the time, e.g.: if 0.2, twenty percent of calls will be randomly affected; if 0.01, one percent of calls; if 1, all calls. |
| `InjectionRateGenerator` | `null`        | Generates the injection rate for a given execution, which the value should be between [0, 1] (inclusive).                                                                                                                        |
| `Enabled`                | `true`        | Determines whether the strategy is enabled or not.                                                                                                                                                                               |
| `EnabledGenerator`       | `null`        | The generator that indicates whether the chaos strategy is enabled for a given execution.                                                                                                                                        |

> [!NOTE]
> If both `InjectionRate` and `InjectionRateGenerator` are specified then `InjectionRate` will be ignored.
>
> If both `Enabled` and `EnabledGenerator` are specified then `Enabled` will be ignored.

[simmy]: https://github.com/Polly-Contrib/Simmy

## Telemetry

The telemetry of chaos strategies is seamlessly integrated with Polly [telemetry infrastructure](../advanced/telemetry.md). The chaos strategies produce the following information events:

- [`Chaos.OnFault`](fault.md#telemetry)
- [`Chaos.OnOutcome`](outcome.md#telemetry)
- [`Chaos.OnLatency`](latency.md#telemetry)
- [`Chaos.OnBehavior`](behavior.md#telemetry)

## Patterns

### Inject chaos selectively

You aim to dynamically adjust the frequency and timing of chaos injection. For instance, in pre-production and test environments, it's sensible to consistently inject chaos. This proactive approach helps in preparing for potential failures. In production environments, however, you may prefer to limit chaos to certain users and tenants, ensuring that regular users remain unaffected. The chaos API offers the flexibility needed to manage these varying scenarios.

Additionally, you have the option to dynamically alter the injection rate and simulate extreme scenarios by setting the injection rate to *1.0 (100%)*. Exercise caution when applying this high rate, restricting it to a subset of tenants and users to avoid rendering the system unusable for regular users.

The following example illustrates how to configure chaos strategies accordingly:

<!-- snippet: chaos-selective -->
```cs
services.AddResiliencePipeline("chaos-pipeline", (builder, context) =>
{
    var environment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();

    builder.AddChaosFault(new ChaosFaultStrategyOptions
    {
        EnabledGenerator = args =>
        {
            // Enable chaos in development and staging environments.
            if (environment.IsDevelopment() || environment.IsStaging())
            {
                return ValueTask.FromResult(true);
            }

            // Enable chaos for specific users or tenants, even in production environments.
            if (ShouldEnableChaos(args.Context))
            {
                return ValueTask.FromResult(true);
            }

            return ValueTask.FromResult(false);
        },
        InjectionRateGenerator = args =>
        {
            if (environment.IsStaging())
            {
                // 1% chance of failure on staging environments.
                return ValueTask.FromResult(0.01);
            }

            if (environment.IsDevelopment())
            {
                // 5% chance of failure on development environments.
                return ValueTask.FromResult(0.05);
            }

            // The context can carry information to help determine the injection rate.
            // For instance, in production environments, you might have certain test users or tenants
            // for whom you wish to inject chaos.
            if (ResolveInjectionRate(args.Context, out double injectionRate))
            {
                return ValueTask.FromResult(injectionRate);
            }

            // No chaos on production environments.
            return ValueTask.FromResult(0.0);
        },
        FaultGenerator = new FaultGenerator()
            .AddException<TimeoutException>()
            .AddException<HttpRequestException>()
    });
});
```
<!-- endSnippet -->

We suggest encapsulating the chaos decisions and injection rate in a shared class, such as `IChaosManager`:

<!-- snippet: chaos-manager -->
```cs
public interface IChaosManager
{
    ValueTask<bool> IsChaosEnabled(ResilienceContext context);

    ValueTask<double> GetInjectionRate(ResilienceContext context);
}
```
<!-- endSnippet -->

This approach allows you to consistently apply and manage chaos-related settings across various chaos strategies by reusing `IChaosManager`. By centralizing the logic for enabling chaos and determining injection rates, you can ensure uniformity and ease of maintenance across your application and reuse it across multiple chaos strategies:

<!-- snippet: chaos-selective-manager -->
```cs
services.AddResiliencePipeline("chaos-pipeline", (builder, context) =>
{
    var chaosManager = context.ServiceProvider.GetRequiredService<IChaosManager>();

    builder
        .AddChaosFault(new ChaosFaultStrategyOptions
        {
            EnabledGenerator = args => chaosManager.IsChaosEnabled(args.Context),
            InjectionRateGenerator = args => chaosManager.GetInjectionRate(args.Context),
            FaultGenerator = new FaultGenerator()
                .AddException<TimeoutException>()
                .AddException<HttpRequestException>()
        })
        .AddChaosLatency(new ChaosLatencyStrategyOptions
        {
            EnabledGenerator = args => chaosManager.IsChaosEnabled(args.Context),
            InjectionRateGenerator = args => chaosManager.GetInjectionRate(args.Context),
            Latency = TimeSpan.FromSeconds(60)
        });
});
```
<!-- endSnippet -->

> [!NOTE]
> An alternative method involves using [`Microsoft.Extensions.AsyncState`](https://www.nuget.org/packages/Microsoft.Extensions.AsyncState) for storing information relevant to chaos injection decisions. This can be particularly useful in frameworks like ASP.NET Core. For instance, you could implement a middleware that retrieves user information from `HttpContext`, assesses the user type, and then stores this data in `IAsyncContext<ChaosUser>`. Subsequently, `IChaosManager` can access `IAsyncContext<ChaosUser>` to retrieve this information. This approach eliminates the need to manually insert such data into `ResilienceContext` for each call within the resilience pipeline, thereby streamlining the process.

### Integrating chaos pipelines

When integrating chaos pipelines with resilience strategies, consider the following approaches:

- Establish a central resilience pipeline and apply it across various pipelines.
- Incorporate chaos strategies into each resilience pipeline individually.

Each approach has its own set of advantages and disadvantages.

#### Integrating chaos pipelines with a central pipeline

To integrate chaos pipelines using a central approach, first define a central chaos pipeline that will be reused across various resilience pipelines:

<!-- snippet: chaos-central-pipeline -->
```cs
services.AddResiliencePipeline("chaos-pipeline", (builder, context) =>
{
    var chaosManager = context.ServiceProvider.GetRequiredService<IChaosManager>();

    builder
        .AddChaosFault(new ChaosFaultStrategyOptions
        {
            FaultGenerator = new FaultGenerator()
                .AddException<TimeoutException>()
                .AddException<HttpRequestException>()
        })
        .AddChaosLatency(new ChaosLatencyStrategyOptions
        {
            Latency = TimeSpan.FromSeconds(60)
        });
});
```
<!-- endSnippet -->

Next, when defining a pipeline, use `ResiliencePipelineProvider<T>` to integrate the chaos pipeline using the `AddPipeline` extension method:

<!-- snippet: chaos-central-pipeline-integration -->
```cs
services.AddResiliencePipeline("my-pipeline-1", (builder, context) =>
{
    var pipelineProvider = context.ServiceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
    var chaosPipeline = pipelineProvider.GetPipeline("chaos-pipeline");

    builder
        .AddRetry(new RetryStrategyOptions())
        .AddTimeout(TimeSpan.FromSeconds(5))
        .AddPipeline(chaosPipeline); // Inject central chaos pipeline

});
```
<!-- endSnippet -->

✅ Central management of the chaos pipeline allows for easy integration into other resilience pipelines.

❌ It's challenging to correlate telemetry between the chaos and resilience pipelines. Telemetry from the chaos pipeline is emitted under `chaos-pipeline`, while the regular resilience pipeline telemetry appears under `my-pipeline-1`.

❌ Fine-tuning the chaos pipeline's behavior to suit specific resilience pipelines is not straightforward. In certain scenarios, you might want to adjust failure rates for a particular pipeline.

#### Integrating chaos pipelines with extensions

In this approach, a helper extension method can be introduced to add a predefined set of chaos strategies to `ResiliencePipelineBuilder<T>`:

<!-- snippet: chaos-extension -->
```cs
// Options that represent the chaos pipeline
public sealed class MyChaosOptions
{
    public ChaosFaultStrategyOptions Fault { get; set; } = new()
    {
        FaultGenerator = new FaultGenerator()
            .AddException<TimeoutException>()
            .AddException<HttpRequestException>()
    };

    public ChaosLatencyStrategyOptions Latency { get; set; } = new()
    {
        Latency = TimeSpan.FromSeconds(60)
    };
}

// Extension for easy integration of the chaos pipeline
public static void AddMyChaos(this ResiliencePipelineBuilder builder, Action<MyChaosOptions>? configure = null)
{
    var options = new MyChaosOptions();
    configure?.Invoke(options);

    builder
        .AddChaosFault(options.Fault)
        .AddChaosLatency(options.Latency);
}
```
<!-- endSnippet -->

The example above:

- Defines `MyChaosOptions`, which encapsulates options for the chaos pipeline with sensible defaults.
- Introduces the `AddMyChaos` extension method for straightforward integration of a custom pipeline into any resilience strategy. It also provides flexibility to modify the pipeline's configuration.

Once the chaos extension is in place, it can be utilized in defining your resilience pipelines:

<!-- snippet: chaos-extension-integration -->
```cs
services.AddResiliencePipeline("my-pipeline-1", (builder, context) =>
{
    builder
        .AddRetry(new RetryStrategyOptions())
        .AddTimeout(TimeSpan.FromSeconds(5))
        .AddMyChaos(); // Use the extension
});

services.AddResiliencePipeline("my-pipeline-2", (builder, context) =>
{
    builder
        .AddRetry(new RetryStrategyOptions())
        .AddTimeout(TimeSpan.FromSeconds(5))
        .AddMyChaos(options =>
        {
            options.Latency.InjectionRate = 0.1; // Override the default injection rate
            options.Latency.Latency = TimeSpan.FromSeconds(10); // Override the default latency
        });
});
```
<!-- endSnippet -->

✅ Enables configuration and customization of chaos strategies for each pipeline, while maintaining a centralized logic.

✅ Simplifies telemetry correlation as chaos strategies share the same pipeline name.

❌ Increased maintenance due to additional code, with flexibility coming at the expense of complexity.

❌ Monitoring multiple chaos pipelines may be necessary to understand their behavior.
