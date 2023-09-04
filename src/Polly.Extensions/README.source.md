# Polly.Extensions Overview

`Polly.Extensions` provides a set of features that streamline the integration of Polly with the standard `IServiceCollection` Dependency Injection (DI) container. It further enhances telemetry by exposing a `ConfigureTelemetry` extension method that enables [logging](https://learn.microsoft.com/dotnet/core/extensions/logging?tabs=command-line) and [metering](https://learn.microsoft.com/dotnet/core/diagnostics/metrics) for all strategies created via DI extension points. 

Below is an example illustrating the usage of `AddResiliencePipeline` extension method:

snippet: add-resilience-pipeline

> [!NOTE]
> Telemetry is enabled by default when utilizing the `AddResiliencePipeline` extension method.

## Telemetry Features

Upon invoking the `ConfigureTelemetry` extension method, Polly begins to emit logs and metrics. Here's an example:

snippet: configure-telemetry

snippet: telemetry-listeners

Alternatively, you can use the `AddResiliencePipeline` extension which automatically adds telemetry:

snippet: add-resilience-pipeline-with-telemetry

### Emitted Metrics

The emitted metrics are emitted under the `Polly` meter name. The subsequent sections provide insights into the metrics produced by Polly. Please note that any custom enriched dimensions are not depicted in the following tables. 

#### resilience-events

- Type: *Counter*
- Description: Emitted upon the occurrence of a resilience event.

Dimensions:

|Name|Description|
|---| ---|
|`event-name`| The name of the emitted event.| 
|`event-severity`| The severity of the event (`Debug`, `Information`, `Warning`, `Error`, `Critical`).|
|`pipeline-name`| The name of the pipeline corresponding to the resilience pipeline.|
|`pipeline-instance`| The instance name of the pipeline corresponding to the resilience pipeline.|
|`strategy-name`| The name of the strategy generating this event.|
|`operation-key`| The operation key associated with the call site. |
|`exception-name`| The full name of the exception assigned to the execution result (`System.InvalidOperationException`). |

#### execution-attempt-duration

- Type: *Histogram*
- Unit: *milliseconds*
- Description: Tracks the duration of execution attempts, produced by `Retry` and `Hedging` resilience strategies.

Dimensions:

|Name|Description|
|---| ---|
|`event-name`| The name of the emitted event.| 
|`event-severity`| The severity of the event (`Debug`, `Information`, `Warning`, `Error`, `Critical`).|
|`pipeline-name`| The name of the pipeline corresponding to the resilience pipeline.|
|`pipeline-instance`| The instance name of the pipeline corresponding to the resilience pipeline.|
|`strategy-name`| The name of the strategy generating this event.|
|`operation-key`| The operation key associated with the call site. |
|`exception-name`| The full name of the exception assigned to the execution result (`System.InvalidOperationException`). |
|`attempt-number`| The execution attempt number, starting at 0 (0, 1, 2). |
|`attempt-handled`| Indicates if the execution outcome was handled. A handled outcome indicates execution failure and the need for retry (`true`, `false`). |

#### pipeline-execution-duration

- Type: *Histogram*
- Unit: *milliseconds*
- Description: Measures the duration and results of resilience pipelines.

Dimensions:

|Name|Description|
|---| ---|
|`pipeline-name`| The name of the pipeline corresponding to the resilience pipeline.|
|`pipeline-instance`| The instance name of the pipeline corresponding to the resilience pipeline.|
|`operation-key`| The operation key associated with the call site. |
|`exception-name`| The full name of the exception assigned to the execution result (`System.InvalidOperationException`). |

### Logs

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
