# Hedging resilience strategy

## About

- **Options**: [`HedgingStrategyOptions<T>`](xref:Polly.Hedging.HedgingStrategyOptions`1)
- **Extensions**: `AddHedging`
- **Strategy Type**: Reactive

---

The hedging strategy enables the re-execution of a user-defined callback if the previous execution takes too long. This approach gives you the option to either run the original callback again or specify a new callback for subsequent hedged attempts. Implementing a hedging strategy can boost the overall responsiveness of the system. However, it's essential to note that this improvement comes at the cost of increased resource utilization. If low latency is not a critical requirement, you may find the [retry strategy](retry.md) is more appropriate.

This strategy also supports multiple [concurrency modes](#concurrency-modes) for added flexibility.

> [!NOTE]
> Please do not start any background work when executing actions using the hedging strategy. This strategy can spawn multiple parallel tasks, and as a result multiple background tasks can be started.

## Usage

<!-- snippet: hedging -->
```cs
// Add hedging with default options.
// See https://www.pollydocs.org/strategies/hedging#defaults for defaults.
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>());

// Add a customized hedging strategy that retries up to 3 times if the execution
// takes longer than 1 second or if it fails due to an exception or returns an HTTP 500 Internal Server Error.
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<SomeExceptionType>()
            .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError),
        MaxHedgedAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        ActionGenerator = args =>
        {
            Console.WriteLine("Preparing to execute hedged action.");

            // Return a delegate function to invoke the original action with the action context.
            // Optionally, you can also create a completely new action to be executed.
            return () => args.Callback(args.ActionContext);
        }
    });

// Subscribe to hedging events.
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
    {
        OnHedging = args =>
        {
            Console.WriteLine($"OnHedging: Attempt number {args.AttemptNumber}");
            return default;
        }
    });
```
<!-- endSnippet -->

## Defaults

| Property            | Default Value                                                              | Description                                                                              |
| ------------------- | -------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| `ShouldHandle`      | Predicate that handles all exceptions except `OperationCanceledException`. | Predicate that determines what results and exceptions are handled by the retry strategy. |
| `MaxHedgedAttempts` | 1                                                                          | The maximum number of hedged actions to use, in addition to the original action.         |
| `Delay`             | 2 seconds                                                                  | The maximum waiting time before spawning a new hedged action.                            |
| `ActionGenerator`   | Returns the original callback that was passed to the hedging strategy.     | Generator that creates hedged actions.                                                   |
| `DelayGenerator`    | `null`                                                                     | Used for generating custom delays for hedging. If `null` then `Delay` is used.           |
| `OnHedging`         | `null`                                                                     | Event that is raised when a hedging is performed.                                        |

You can use the following special values for `Delay` or in `DelayGenerator`:

- `0 seconds` - the hedging strategy immediately creates a total of `MaxHedgedAttempts` and completes when the fastest acceptable result is available.
- `-1 millisecond` - this value indicates that the strategy does not create a new hedged task before the previous one completes. This enables scenarios where having multiple concurrent hedged tasks can cause side effects.

## Concurrency modes

In the sections below, explore the different concurrency modes available in the hedging strategy. The behavior is primarily controlled by the `Delay` property value.

### Latency mode

When the `Delay` property is set to a value greater than zero, the hedging strategy operates in latency mode. In this mode, additional executions are triggered when the initial ones take too long to complete. By default, the `Delay` is set to 2 seconds.

- The primary execution is initiated.
- If the initial execution either fails or takes longer than the `Delay` to complete, a new execution is initiated.
- If the first two executions fail or exceed the `Delay` (calculated from the last initiated execution), another execution is triggered.
- The final result is the result of fastest successful execution.
- If all executions fail, the final result will be the first failure encountered.

#### Latency: happy path sequence diagram

The hedging strategy does not trigger because the response arrives faster than the threshold.

```mermaid
sequenceDiagram
    autonumber
    actor C as Caller
    participant P as Pipeline
    participant H as Hedging
    participant HUC as HedgedUserCallback

    C->>P: Calls ExecuteAsync
    P->>H: Calls ExecuteCore
    Note over H: Wait start

    activate H
    H->>+HUC: Invokes
    HUC-->>HUC: Processes request
    HUC->>-H: Returns result
    deactivate H

    Note over H: Wait end
    H->>P: Returns result
    P->>C: Returns result
```

#### Latency: unhappy path sequence diagram

The hedging strategy triggers because the response arrives slower than the threshold.

```mermaid
sequenceDiagram
    autonumber
    actor C as Caller
    participant P as Pipeline
    participant H as Hedging
    participant D as HedgedUserCallback

    C->>P: Calls ExecuteAsync
    P->>H: Calls ExecuteCore
    Note over H: Wait start

    activate H
    activate D
    H->>D: Invokes (R1)
    D-->>D: Processes R1<br/> slowly ...
    Note over H: Wait end

    H->>D: Invokes (R2)
    activate D
    D-->>D: Processes R2<br/> quickly
    D->>H: Returns result (R2)
    deactivate D
    H->>D: Propagates cancellation (R1)
    deactivate H

    deactivate D
    H->>P: Returns result (R2)
    P->>C: Returns result (R2)
```

### Fallback mode

In fallback mode, the `Delay` value should be less than `TimeSpan.Zero`. This mode allows only a single execution to proceed at a given time.

- An execution is initiated, and the strategy waits for its completion.
- If the initial execution fails, new one is initiated.
- The final result will be the first successful execution.
- If all executions fail, the final result will be the first failure encountered.

#### Fallback: happy path sequence diagram

The hedging strategy triggers because the first attempt fails. It succeeds because the retry attempts do not exceed the `MaxHedgedAttempts`.

```mermaid
sequenceDiagram
    autonumber
    actor C as Caller
    participant P as Pipeline
    participant H as Hedging
    participant HUC as HedgedUserCallback

    C->>P: Calls ExecuteAsync
    P->>H: Calls ExecuteCore
    activate H

    H->>+HUC: Invokes (R1)
    HUC-->>HUC: Processes R1
    HUC->>-H: Fails (R1)

    H->>+HUC: Invokes (R2)
    HUC-->>HUC: Processes R2
    HUC->>-H: Returns result (R2)

    deactivate H
    H->>P: Returns result (R2)
    P->>C: Returns result (R2)
```

#### Fallback: unhappy path sequence diagram

The hedging strategy triggers because the first attempt fails. It fails because all retry attempts failed as well.

```mermaid
sequenceDiagram
    autonumber
    actor C as Caller
    participant P as Pipeline
    participant H as Hedging
    participant HUC as HedgedUserCallback

    C->>P: Calls ExecuteAsync
    P->>H: Calls ExecuteCore
    activate H

    H->>+HUC: Invokes (R1)
    HUC-->>HUC: Processes R1
    HUC->>-H: Fails (R1)

    H->>+HUC: Invokes (R2)
    HUC-->>HUC: Processes R2
    HUC->>-H: Fails (R2)

    deactivate H
    H->>P: Propagates failure (R1)
    P->>C: Propagates failure (R1)
```

### Parallel mode

The hedging strategy operates in parallel mode when the `Delay` property is set to `TimeSpan.Zero`. In this mode, all executions are initiated simultaneously, and the strategy waits for the fastest completion.

> [!IMPORTANT]
> Use this mode only when absolutely necessary, as it consumes the most resources, particularly when the hedging strategy uses remote resources such as remote HTTP services.

- All executions are initiated simultaneously, adhering to the `MaxHedgedAttempts` limit.
- The final result will be the fastest successful execution.
- If all executions fail, the final result will be the first failure encountered.

#### Parallel: happy path sequence diagram

The hedging strategy triggers because the `Delay` is set to zero. It succeeds because one of the requests succeeds.

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant H as Hedging
    participant HUC as HedgedUserCallback

    C->>P: Calls ExecuteAsync
    P->>H: Calls ExecuteCore
    activate H

    par
    H->>HUC: Invokes (R1)
    activate HUC

    and
    H->>+HUC: Invokes (R2)

    end

    HUC-->>HUC: Processes R1<br/> slowly ...
    HUC-->>HUC: Processes R2<br/> quickly ...
    HUC->>-H: Returns result (R2)

    H->>HUC: Propagates cancellation (R1)
    deactivate HUC

    deactivate H
    H->>P: Returns result (R2)
    P->>C: Returns result (R2)
```

#### Parallel: unhappy path sequence diagram

The hedging strategy triggers because the `Delay` is set to zero. It fails because all requests fail.

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant H as Hedging
    participant HUC as HedgedUserCallback

    C->>P: Calls ExecuteAsync
    P->>H: Calls ExecuteCore
    activate H

    par
    H->>HUC: Invokes (R1)
    activate HUC

    and
    H->>+HUC: Invokes (R2)

    end

    HUC-->>HUC: Processes R1
    HUC-->>HUC: Processes R2
    HUC->>-H: Fails (R2)
    HUC->>-H: Fails (R1)

    deactivate H
    H->>P: Propagates failure (R2)
    P->>C: Propagates failure (R2)
```

### Dynamic mode

In dynamic mode, you have the flexibility to control how the hedging strategy behaves during each execution. This control is achieved through the `DelayGenerator` property.

> [!NOTE]
> The `Delay` property is disregarded when `DelayGenerator` is set.

Example scenario:

- First, initiate the first two executions in parallel mode.
- Subsequently, switch to fallback mode for additional executions.

To configure hedging according to the above scenario, use the following code:

<!-- snippet: hedging-dynamic-mode -->
```cs
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddHedging(new()
    {
        MaxHedgedAttempts = 3,
        DelayGenerator = args =>
        {
            var delay = args.AttemptNumber switch
            {
                0 or 1 => TimeSpan.Zero, // Parallel mode
                _ => TimeSpan.FromSeconds(-1) // switch to Fallback mode
            };

            return new ValueTask<TimeSpan>(delay);
        }
    });
```
<!-- endSnippet -->

With this configuration, the hedging strategy:

- Initiates a maximum of `4` executions. This includes initial action and an additional 3 attempts.
- Allows the first two executions to proceed in parallel, while the third and fourth executions follow the fallback mode.

#### Dynamic: happy path sequence diagram

The hedging strategy triggers and switches between modes due to our `DelayGenerator`. It succeeds because the last request succeeds.

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant H as Hedging
    participant DG as DelayGenerator
    participant HUC as HedgedUserCallback

    C->>P: Calls ExecuteAsync
    P->>H: Calls ExecuteCore
    activate H

    Note over H: Parallel mode
    par
    H->>DG: Gets delay
    DG->>H: 0 second
    H->>HUC: Invokes (R1)
    activate HUC
    and
    H ->> DG: Gets delay
    DG ->> H: 0 second
    H->>+HUC: Invokes (R2)
    end

    HUC-->>HUC: Processes R1
    HUC-->>HUC: Processes R2
    HUC->>-H: Fails (R2)
    HUC->>-H: Fails (R1)

    Note over H: Fallback mode

    H->>DG: Gets delay
    DG->>H: -1 second
    H->>+HUC: Invokes (R3)
    HUC-->>HUC: Processes R3
    HUC->>-H: Fails (R3)

    H->>DG: Gets delay
    DG->>H: -1 second
    H->>+HUC: Invokes (R4)
    HUC-->>HUC: Processes R4
    HUC->>-H: Returns result (R4)

    deactivate H
    H->>P: Returns result (R4)
    P->>C: Returns result (R4)
```

#### Dynamic: unhappy path sequence diagram

The hedging strategy triggers and switches between modes due our `DelayGenerator`. It fails because all requests fail.

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant H as Hedging
    participant DG as DelayGenerator
    participant HUC as HedgedUserCallback

    C->>P: Calls ExecuteAsync
    P->>H: Calls ExecuteCore
    activate H

    Note over H: Parallel mode
    par
    H->>DG: Gets delay
    DG->>H: 0 second
    H->>HUC: Invokes (R1)
    activate HUC
    and
    H->>DG: Gets delay
    DG->>H: 0 second
    H->>+HUC: Invokes (R2)
    end

    HUC-->>HUC: Processes R1
    HUC-->>HUC: Processes R2
    HUC->>-H: Fails (R2)
    HUC->>-H: Fails (R1)

    Note over H: Fallback mode

    H->>DG: Gets delay
    DG->>H: -1 second
    H->>+HUC: Invokes (R3)
    HUC-->>HUC: Processes R3
    HUC->>-H: Fails (R3)

    H -> DG: Gets delay
    DG->>H: -1 second
    H->>+HUC: Invokes (R4)
    HUC-->>HUC: Processes R4
    HUC->>-H: Fails (R4)

    deactivate H
    H->>P: Propagates failure (R3)
    P->>C: Propagates failure (R3)
```

## Action generator

The hedging options include an `ActionGenerator` property, allowing you to customize the actions executed during hedging. By default, the `ActionGenerator` returns the original callback passed to the strategy. The original callback also includes any logic introduced by subsequent resilience strategies. For more advanced scenarios, the `ActionGenerator` can be used to return entirely new hedged actions, as demonstrated in the example below:

<!-- snippet: hedging-action-generator -->
```cs
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddHedging(new()
    {
        ActionGenerator = args =>
        {
            // You can access data from the original (primary) context here
            var customData = args.PrimaryContext.Properties.GetValue(customDataKey, "default-custom-data");

            Console.WriteLine($"Hedging, Attempt: {args.AttemptNumber}, Custom Data: {customData}");

            // Here, we can access the original callback and return it or return a completely new action
            var callback = args.Callback;

            // A function that returns a ValueTask<Outcome<HttpResponseMessage>> is required.
            return async () =>
            {
                try
                {
                    // A dedicated ActionContext is provided for each hedged action.
                    // It comes with a separate CancellationToken created specifically for this hedged attempt,
                    // which can be cancelled later if needed.
                    //
                    // Note that the "MyRemoteCallAsync" call won't have any additional resilience applied.
                    // You are responsible for wrapping it with any additional resilience pipeline.
                    var response = await MyRemoteCallAsync(args.ActionContext.CancellationToken);

                    return Outcome.FromResult(response);
                }
                catch (Exception e)
                {
                    // Note: All exceptions should be caught and converted to Outcome.
                    return Outcome.FromException<HttpResponseMessage>(e);
                }
            };
        }
    });
```
<!-- endSnippet -->

### Action generator: sequence diagram

```mermaid
sequenceDiagram
    autonumber
    actor C as Caller
    participant P as Pipeline
    participant H as Hedging
    participant AG as ActionGenerator
    participant UC as UserCallback
    participant HUC as HedgedUserCallback

    C->>P: Calls ExecuteAsync
    P->>H: Calls ExecuteCore

    activate H
    H->>+UC: Invokes (R1)
    UC-->>UC: Processes R1
    UC->>-H: Fails (R1)
    H->>+AG: Invokes
    AG->>-H: Returns factory
    H-->>H: Invokes factory

    H->>+HUC: Invokes (R2)
    HUC-->>HUC: Processes R2
    HUC->>-H: Fails (R2)

    H-->>H: Invokes factory
    H->>+HUC: Invokes (R3)
    HUC-->>HUC: Processes R3
    HUC->>-H: Returns result (R3)
    deactivate H

    H->>P: Returns result (R3)
    P->>C: Returns result (R3)
```

### Parameterized callbacks and action generator

When you have control over the callbacks that the resilience pipeline receives, you can parameterize them. This flexibility allows for reusing the callbacks within an action generator.

A common use case is with [`DelegatingHandler`](https://learn.microsoft.com/aspnet/web-api/overview/advanced/http-message-handlers). Here, you can parameterize the `HttpRequestMessage`:

<!-- snippet: hedging-handler -->
```cs
internal class HedgingHandler : DelegatingHandler
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public HedgingHandler(ResiliencePipeline<HttpResponseMessage> pipeline)
    {
        _pipeline = pipeline;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = ResilienceContextPool.Shared.Get(cancellationToken);

        // Store the incoming request in the context
        context.Properties.Set(ResilienceKeys.RequestMessage, request);

        try
        {
            return await _pipeline.ExecuteAsync(async cxt =>
            {
                // Allow the pipeline to use request message that was stored in the context.
                // This allows replacing the request message with a new one in the resilience pipeline.
                request = cxt.Properties.GetValue(ResilienceKeys.RequestMessage, request);

                return await base.SendAsync(request, cxt.CancellationToken);
            },
            context);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }
}
```
<!-- endSnippet -->

Where `ResilienceKeys` is defined as:

<!-- snippet: hedging-resilience-keys -->
```cs
internal static class ResilienceKeys
{
    public static readonly ResiliencePropertyKey<HttpRequestMessage> RequestMessage = new("MyFeature.RequestMessage");
}
```
<!-- endSnippet -->

In your `ActionGenerator`, you can easily provide your own `HttpRequestMessage` to `ActionContext`, and the original callback will use it:

<!-- snippet: hedging-parametrized-action-generator -->
```cs
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddHedging(new()
    {
        ActionGenerator = args =>
        {
            if (!args.PrimaryContext.Properties.TryGetValue(ResilienceKeys.RequestMessage, out var request))
            {
                throw new InvalidOperationException("The request message must be provided.");
            }

            // Prepare a new request message for the callback, potentially involving:
            //
            // - Cloning the request message
            // - Providing alternate endpoint URLs
            request = PrepareRequest(request);

            // Override the request message in the action context
            args.ActionContext.Properties.Set(ResilienceKeys.RequestMessage, request);

            // Then, execute the original callback
            return () => args.Callback(args.ActionContext);
        }
    });
```
<!-- endSnippet -->
