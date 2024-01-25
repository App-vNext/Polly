# Chaos engineering with Simmy

> [!IMPORTANT]
> This documentation page describes an upcoming feature of Polly.

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

// 2% of invocations will be injected with chaos
const double InjectionRate = 0.02;

builder
    .AddChaosLatency(InjectionRate, TimeSpan.FromMinutes(1)) // Inject a chaos latency to executions
    .AddChaosFault(InjectionRate, () => new InvalidOperationException("Injected by chaos strategy!")) // Inject a chaos fault to executions
    .AddChaosOutcome(InjectionRate, () => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)) // Inject a chaos outcome to executions
    .AddChaosBehavior(0.001, cancellationToken => RestartRedisAsync(cancellationToken)); // Inject a chaos behavior to executions
```
<!-- endSnippet -->

> [!NOTE]
> It is usual to place the chaos strategy as the last strategy in the resilience pipeline. By placing the chaos strategies as last, they subvert the usual outbound call at the last minute, substituting their fault or adding extra latency, etc. The existing resilience strategies - further out in the `ResiliencePipeline` - still apply, so you can test how the Polly resilience strategies you have configured handle the chaos/faults injected by Simmy.

## Motivation

There are a lot of questions when it comes to chaos engineering and making sure that a system is actually ready to face the worst possible scenarios:

* Is my system resilient enough?
* Am I handling the right exceptions/scenarios?
* How will my system behave if X happens?
* How can I test without waiting for a handled (or even unhandled) exception to happen in my production environment?

Using Polly helps introduce resilience to a project, but we don't want to have to wait for expected or unexpected failures to test it out. A resilience could be wrongly implemented; testing the scenarios is not straightforward; and mocking failure of some dependencies (for example a cloud SaaS or PaaS service) is not always straightforward.

### What is needed to simulate chaotic scenarios?

* A way to simulate failures of dependencies (any service dependency for example).
* Define when to fail based on some external factors - maybe global configuration or some rule.
* A way to revert easily, to control the blast radius.
* To be production grade, to run this in a production or near-production system with automation.

## Chaos strategies

Chaos strategies (formerly known as Monkey strategies) are in essence a [Resilience strategy](../strategies/index.md#built-in-strategies), which means, as a *Resilience Strategy* is the minimum unit of resilience for Polly, a *Chaos Strategy* is the minimum unit of chaos for Simmy.

### Built-in strategies

| Strategy                | Reactive | What does the strategy do?                                           |
|-------------------------|----------|----------------------------------------------------------------------|
| [Fault](fault.md)       | No       | Injects exceptions in your system.                                   |
| [Outcome](outcome.md)   | Yes      | Injects fake outcomes (results or exceptions) in your system.        |
| [Latency](latency.md)   | No       | Injects latency into executions before the calls are made.           |
| [Behavior](behavior.md) | No       | Allows you to inject *any* extra behavior, before a call is placed.  |

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

## Patterns

### Inject chaos selectively

You aim to dynamically adjust the frequency and timing of chaos injection. For instance, in pre-production and test environments, it's sensible to consistently inject chaos. This proactive approach helps in preparing for potential failures. In production environments, however, you may prefer to limit chaos to certain users and tenants, ensuring that regular users remain unaffected. The Simmy API offers the flexibility needed to manage these varying scenarios.

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
    bool IsChaosEnabled(ResilienceContext context);

    double GetInjectionRate(ResilienceContext context);
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
            EnabledGenerator = args => ValueTask.FromResult(chaosManager.IsChaosEnabled(args.Context)),
            InjectionRateGenerator = args => ValueTask.FromResult(chaosManager.GetInjectionRate(args.Context)),
            FaultGenerator = new FaultGenerator()
                .AddException<TimeoutException>()
                .AddException<HttpRequestException>()
        })
        .AddChaosLatency(new ChaosLatencyStrategyOptions
        {
            EnabledGenerator = args => ValueTask.FromResult(chaosManager.IsChaosEnabled(args.Context)),
            InjectionRateGenerator = args => ValueTask.FromResult(chaosManager.GetInjectionRate(args.Context)),
            Latency = TimeSpan.FromSeconds(60)
        });
});
```
<!-- endSnippet -->
