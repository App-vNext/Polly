# Telemetry

Starting with version 8, Polly provides telemetry for all built-in resilience strategies.

## Usage

To enable telemetry in Polly, add the `Polly.Extensions` package to your project:

```sh
dotnet add package Polly.Extensions
```

Afterwards, you can use the `ConfigureTelemetry(...)` extension method on the `ResiliencePipelineBuilder`:

<!-- snippet: configure-telemetry -->
```cs
var telemetryOptions = new TelemetryOptions
{
    // Configure logging
    LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole())
};

// Configure enrichers
telemetryOptions.MeteringEnrichers.Add(new MyMeteringEnricher());

// Configure telemetry listeners
telemetryOptions.TelemetryListeners.Add(new MyTelemetryListener());

var builder = new ResiliencePipelineBuilder()
    .AddTimeout(TimeSpan.FromSeconds(1))
    .ConfigureTelemetry(telemetryOptions) // This method enables telemetry in the builder
    .Build();
```
<!-- endSnippet -->

The `MyTelemetryListener` and `MyMeteringEnricher` is implemented as:

<!-- snippet: telemetry-listeners -->
```cs
internal class MyTelemetryListener : TelemetryListener
{
    public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
    {
        Console.WriteLine($"Telemetry event occurred: {args.Event.EventName}");
    }
}

internal class MyMeteringEnricher : MeteringEnricher
{
    public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
    {
        context.Tags.Add(new("my-custom-tag", "custom-value"));
    }
}
```
<!-- endSnippet -->

Alternatively, you can use the `AddResiliencePipeline(...)` extension method which automatically enables telemetry for defined pipeline:

<!-- snippet: add-resilience-pipeline-with-telemetry -->
```cs
var serviceCollection = new ServiceCollection()
    .AddLogging(builder => builder.AddConsole())
    .AddResiliencePipeline("my-strategy", builder => builder.AddTimeout(TimeSpan.FromSeconds(1)))
    .Configure<TelemetryOptions>(options =>
    {
        // Configure enrichers
        options.MeteringEnrichers.Add(new MyMeteringEnricher());

        // Configure telemetry listeners
        options.TelemetryListeners.Add(new MyTelemetryListener());
    });
```
<!-- endSnippet -->

## Metrics

The metrics are emitted under the `Polly` meter name. The subsequent sections provide insights into the metrics produced by Polly. Please note that any custom enriched tags are not depicted in the following tables.

Every telemetry event has the following tags:

- `pipeline.name`: Optional, comes from `ResiliencePipelineBuilder.Name`.
- `pipeline.instance`: Optional, comes from `ResiliencePipelineBuilder.InstanceName`.
- `strategy.name`: Optional, comes from `RetryStrategyOptions.Name`.
- `operation.key`: Optional, comes from `ResilienceContext.OperationKey`.

The sample below demonstrates how to assign these tags:

<!-- snippet: telemetry-tags -->
```cs
var builder = new ResiliencePipelineBuilder();
builder.Name = "my-name";
builder.Name = "my-instance-name";

builder.AddRetry(new RetryStrategyOptions
{
    // The default value is "Retry"
    Name = "my-retry-name"
});

ResiliencePipeline pipeline = builder.Build();

// Create resilience context with operation key
ResilienceContext resilienceContext = ResilienceContextPool.Shared.Get("my-operation-key");

// Execute the pipeline with the context
pipeline.Execute(
    context =>
    {
        // Your code comes here
    },
    resilienceContext);
```
<!-- endSnippet -->

> [!NOTE]
> Beware of using very large or unbounded combinations of tag values for the tags above. See [best practices](https://learn.microsoft.com/dotnet/core/diagnostics/metrics-instrumentation#best-practices-3) for more details.

These values are subsequently reflected in the following metering instruments exposed by Polly:

### Instrument: `resilience.polly.strategy.events`

- Type: *Counter*
- Numerical type of measurement: *int*
- Description: Emitted upon the occurrence of a resilience event.

Tags:

|Name|Description|
|---| ---|
|`event.name`| The name of the emitted event.|
|`event.severity`| The severity of the event (`Debug`, `Information`, `Warning`, `Error`, `Critical`).|
|`pipeline.name`| The name of the pipeline corresponding to the resilience pipeline.|
|`pipeline.instance`| The instance name of the pipeline corresponding to the resilience pipeline.|
|`strategy.name`| The name of the strategy generating this event.|
|`operation.key`| The operation key associated with the call site. |
|`exception.type`| The full name of the exception assigned to the execution result (`System.InvalidOperationException`). |

#### Event names

The `event.name` tag is reported by individual resilience strategies. The built-in strategies report the following events:

- `OnRetry`
- `OnFallback`
- `OnHedging`
- `OnTimeout`
- `OnCircuitClosed`
- `OnCircuitOpened`
- `OnCircuitHalfOpened`
- `OnRateLimiterRejected`

### Instrument: `resilience.polly.strategy.attempt.duration`

- Type: *Histogram*
- Unit: *milliseconds*
- Numerical type of measurement: *double*
- Description: Tracks the duration of execution attempts, produced by `Retry` and `Hedging` resilience strategies.

Tags:

|Name|Description|
|---| ---|
|`event.name`| The name of the emitted event. Currently, the event name is always `ExecutionAttempt`. |
|`event.severity`| The severity of the event (`Debug`, `Information`, `Warning`, `Error`, `Critical`).|
|`pipeline.name`| The name of the pipeline corresponding to the resilience pipeline.|
|`pipeline.instance`| The instance name of the pipeline corresponding to the resilience pipeline.|
|`strategy.name`| The name of the strategy generating this event.|
|`operation.key`| The operation key associated with the call site. |
|`exception.type`| The full name of the exception assigned to the execution result (`System.InvalidOperationException`). |
|`attempt.number`| The execution attempt number, starting at 0 (0, 1, 2, etc.). |
|`attempt.handled`| Indicates if the execution outcome was handled. A handled outcome indicates execution failure and the need for retry (`true`, `false`). |

### Instrument: `resilience.polly.pipeline.duration`

- Type: *Histogram*
- Unit: *milliseconds*
- Numerical type of measurement: *double*
- Description: Measures the duration of resilience pipelines.

Tags:

|Name|Description|
|---| ---|
|`pipeline.name`| The name of the pipeline corresponding to the resilience pipeline.|
|`pipeline.instance`| The instance name of the pipeline corresponding to the resilience pipeline.|
|`operation.key`| The operation key associated with the call site. |
|`exception.type`| The full name of the exception assigned to the execution result (`System.InvalidOperationException`). |

## Logs

Logs are registered under the `Polly` logger name. Here are some examples of the logs:

``` text
// This log is recorded whenever a resilience event occurs. EventId = 0
Resilience event occurred. EventName: '{EventName}', Source: '{PipelineName}/{PipelineInstance}/{StrategyName}', Operation Key: '{OperationKey}', Result: '{Result}'

// This log is recorded when a resilience pipeline begins executing. EventId = 1
Resilience pipeline executing. Source: '{PipelineName}/{PipelineInstance}', Operation Key: '{OperationKey}'

// This log is recorded when a resilience pipeline finishes execution. EventId = 2
Resilience pipeline executed. Source: '{PipelineName}/{PipelineInstance}', Operation Key: '{OperationKey}', Result: '{Result}', Execution Health: '{ExecutionHealth}', Execution Time: {ExecutionTime}ms

// This log is recorded upon the completion of every execution attempt. EventId = 3
Execution attempt. Source: '{PipelineName}/{PipelineInstance}/{StrategyName}', Operation Key: '{OperationKey}', Result: '{Result}', Handled: '{Handled}', Attempt: '{Attempt}', Execution Time: '{ExecutionTimeMs}'
```

## Emitting telemetry events

Each resilience strategy can generate telemetry data through the [`ResilienceStrategyTelemetry`](xref:Polly.Telemetry.ResilienceStrategyTelemetry) API. Polly encapsulates event details as [`TelemetryEventArguments`](xref:Polly.Telemetry.TelemetryEventArguments`2) and emits them via `TelemetryListener`.

To leverage this telemetry data, users should assign a `TelemetryListener` instance to `ResiliencePipelineBuilder.TelemetryListener` and then consume the `TelemetryEventArguments`.

For common scenarios, it is expected that users would make use of `Polly.Extensions`. This extension enables telemetry configuration through the `ResiliencePipelineBuilder.ConfigureTelemetry(...)` method, which processes `TelemetryEventArguments` to generate logs and metrics.
