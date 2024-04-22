# Timeout resilience strategy

## About

- **Option(s)**:
  - [`TimeoutStrategyOptions`](xref:Polly.Timeout.TimeoutStrategyOptions)
- **Extension(s)**:
  - `AddTimeout`
- **Exception(s)**:
  - `TimeoutRejectedException`: Thrown when a delegate executed through a timeout strategy does not complete before the timeout.

---

The timeout **proactive** resilience strategy cancels the execution if it does not complete within the specified timeout period. If the execution is canceled by the timeout strategy, it throws a `TimeoutRejectedException`. The timeout strategy operates by wrapping the incoming cancellation token with a new one. Should the original token be canceled, the timeout strategy will transparently honor the original cancellation token without throwing a `TimeoutRejectedException`.

> [!IMPORTANT]
> It is crucial that the user's callback respects the cancellation token. If it does not, the callback will continue executing even after a cancellation request, thereby ignoring the cancellation.

## Usage

<!-- snippet: timeout -->
```cs
// To add a timeout with a custom TimeSpan duration
new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromSeconds(3));

// Timeout using the default options.
// See https://www.pollydocs.org/strategies/timeout#defaults for defaults.
var optionsDefaults = new TimeoutStrategyOptions();

// To add a timeout using a custom timeout generator function
var optionsTimeoutGenerator = new TimeoutStrategyOptions
{
    TimeoutGenerator = static args =>
    {
        // Note: the timeout generator supports asynchronous operations
        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
    }
};

// To add a timeout and listen for timeout events
var optionsOnTimeout = new TimeoutStrategyOptions
{
    TimeoutGenerator = static args =>
    {
        // Note: the timeout generator supports asynchronous operations
        return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
    },
    OnTimeout = static args =>
    {
        Console.WriteLine($"{args.Context.OperationKey}: Execution timed out after {args.Timeout.TotalSeconds} seconds.");
        return default;
    }
};

// Add a timeout strategy with a TimeoutStrategyOptions instance to the pipeline
new ResiliencePipelineBuilder().AddTimeout(optionsDefaults);
```
<!-- endSnippet -->

Example execution:

<!-- snippet: timeout-execution -->
```cs
var pipeline = new ResiliencePipelineBuilder()
    .AddTimeout(TimeSpan.FromSeconds(3))
    .Build();

HttpResponseMessage httpResponse = await pipeline.ExecuteAsync(
      async ct =>
      {
          // Execute a delegate that takes a CancellationToken as an input parameter.
          return await httpClient.GetAsync(endpoint, ct);
      },
      cancellationToken);
```
<!-- endSnippet -->

### Failure handling

It might not be obvious at the first glance what is the difference between these two techniques:

<!-- snippet: timeout-handling-failure -->
```cs
var withOnTimeout = new ResiliencePipelineBuilder()
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(2),
        OnTimeout = args =>
        {
            Console.WriteLine("Timeout limit has been exceeded");
            return default;
        }
    }).Build();

var withoutOnTimeout = new ResiliencePipelineBuilder()
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(2)
    }).Build();

try
{
    await withoutOnTimeout.ExecuteAsync(UserDelegate, CancellationToken.None);
}
catch (TimeoutRejectedException)
{
    Console.WriteLine("Timeout limit has been exceeded");
}
```
<!-- endSnippet -->

## Defaults

| Property           | Default Value | Description                                                                                                                          |
|--------------------|---------------|--------------------------------------------------------------------------------------------------------------------------------------|
| `Timeout`          | 30 seconds    | Defines a **static** period within which the delegate should complete, otherwise it will be cancelled.                               |
| `TimeoutGenerator` | `null`        | This delegate allows you to **dynamically** calculate the timeout period by utilizing information that is only available at runtime. |
| `OnTimeout`        | `null`        | If provided then it will be invoked after the timeout occurred.                                                                      |

> [!NOTE]
> If both `Timeout` and `TimeoutGenerator` are specified then `Timeout` will be ignored.

### `OnTimeout` versus catching `TimeoutRejectedException`

The `OnTimeout` user-provided delegate is called just before the strategy throws the `TimeoutRejectedException`. This delegate receives a parameter which allows you to access the `Context` object as well as the `Timeout`:

- Accessing the `Context` is also possible via a different `Execute{Async}` overload.
- Accessing the `Timeout` can be useful if you are using the `TimeoutGenerator` property rather than the `Timeout` property.

So, what is the purpose of the `OnTimeout` in case of static timeout settings?

The `OnTimeout` delegate can be useful when you define a resilience pipeline which consists of multiple strategies. For example you have a timeout as the inner strategy and a retry as the outer strategy. If the retry is defined to handle `TimeoutRejectedException`, that means the `Execute{Async}` may or may not throw that exception depending on future attempts. So, if you want to get notification about the fact that a timeout has occurred, you have to provide a delegate to the `OnTimeout` property.

## Telemetry

The timeout strategy reports the following telemetry events:

| Event Name  | Event Severity | When?                                                   |
|-------------|----------------|---------------------------------------------------------|
| `OnTimeout` | `Error`        | Just before the strategy calls the `OnTimeout` delegate |

Here are some sample events:

```none
Resilience event occurred. EventName: 'OnTimeout', Source: '(null)/(null)/Timeout', Operation Key: '', Result: ''
Resilience event occurred. EventName: 'OnTimeout', Source: 'MyPipeline/MyPipelineInstance/MyTimeoutStrategy', Operation Key: 'MyTimeoutGuardedOperation', Result: ''
```

> [!NOTE]
> Please note that the `OnTimeout` telemetry event will be reported **only if** the timeout strategy cancels the provided callback execution.
>
> So, if the callback either finishes on time or throws an exception then there will be no telemetry emitted.
>
> Also remember that the `Result` will be **always empty** for the `OnTimeout` telemetry event.

For further information please check out the [telemetry page](https://www.pollydocs.org/advanced/telemetry).

## Diagrams

### Happy path sequence diagram

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant T as Timeout
    participant D as DecoratedUserCallback

    C->>P: Calls ExecuteAsync
    P->>T: Calls ExecuteCore
    T->>+D: Invokes
    D-->>D: Performs <br/>long-running <br/>operation
    D->>-T: Returns result
    T->>P: Returns result
    P->>C: Returns result
```

### Unhappy path sequence diagram

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant T as Timeout
    participant D as DecoratedUserCallback

    C->>P: Calls ExecuteAsync
    P->>T: Calls ExecuteCore
    T->>+D: Invokes
    activate T
    activate D
    D-->>D: Performs <br/>long-running <br/>operation
    T-->>T: Times out
    deactivate T

    T->>D: Propagates cancellation
    D-->>D: Cancellation of callback
    D->>T: Cancellation finished
    deactivate D

    T->>P: Throws <br/>TimeoutRejectedException
    P->>C: Propagates exception
```

> [!IMPORTANT]
> Notice that the timeout waits until the callback is cancelled before throwing `TimeoutRejectedException`. Therefore it's important for the callbacks to respect the cancellation token passed to the execution. If the cancellation token is not correctly respected, the timeout is unnecessarily delayed.

## Anti-patterns

Over the years, many developers have used Polly in various ways. Some of these
recurring patterns may not be ideal. The sections below highlight anti-patterns to avoid.

### Ignoring Cancellation Token

❌ DON'T

Ignore the cancellation token provided by the resilience pipeline:

<!-- snippet: timeout-anti-pattern-cancellation-token -->
```cs
var pipeline = new ResiliencePipelineBuilder()
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();

await pipeline.ExecuteAsync(
    async innerToken => await Task.Delay(TimeSpan.FromSeconds(3), outerToken), // The delay call should use innerToken
    outerToken);
```
<!-- endSnippet -->

**Reasoning**:

The provided callback ignores the `innerToken` passed from the pipeline and instead uses the `outerToken`. For this reason, the cancelled `innerToken` is ignored, and the callback is not cancelled within 1 second.

✅ DO

Respect the cancellation token provided by the pipeline:

<!-- snippet: timeout-pattern-cancellation-token -->
```cs
var pipeline = new ResiliencePipelineBuilder()
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();

await pipeline.ExecuteAsync(
    static async innerToken => await Task.Delay(TimeSpan.FromSeconds(3), innerToken),
    outerToken);
```
<!-- endSnippet -->

**Reasoning**:

The provided callback respects the `innerToken` provided by the pipeline, and as a result, the callback is correctly cancelled by the timeout strategy after 1 second plus `TimeoutRejectedException` is thrown.
