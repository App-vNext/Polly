# Fault chaos strategy

## About

- **Options**: [`FaultStrategyOptions`](xref:Polly.Simmy.Fault.FaultStrategyOptions)
- **Extensions**: `AddChaosFault`
- **Strategy Type**: Proactive

---

The fault chaos strategy is designed to introduce faults (exceptions) into the system, simulating real-world scenarios where operations might fail unexpectedly. It is configurable to inject specific types of exceptions or use custom logic to generate faults dynamically.

## Usage

<!-- snippet: chaos-fault-usage -->
```cs
// 10% of invocations will be randomly affected.
var optionsBasic = new FaultStrategyOptions
{
    FaultGenerator = static args => new ValueTask<Exception?>(new InvalidOperationException("Dummy exception")),
    Enabled = true,
    InjectionRate = 0.1
};

// To use a custom function to generate the fault to inject.
var optionsWithFaultGenerator = new FaultStrategyOptions
{
    FaultGenerator = static args =>
    {
        Exception? exception = args.Context.OperationKey switch
        {
            "DataLayer" => new TimeoutException(),
            "ApplicationLayer" => new InvalidOperationException(),
            _ => null // When the fault generator returns null the strategy won't inject any fault and it will just invoke the user's callback
        };

        return new ValueTask<Exception?>(exception);
    },
    Enabled = true,
    InjectionRate = 0.1
};

// To get notifications when a fault is injected
var optionsOnFaultInjected = new FaultStrategyOptions
{
    FaultGenerator = static args => new ValueTask<Exception?>(new InvalidOperationException("Dummy exception")),
    Enabled = true,
    InjectionRate = 0.1,
    OnFaultInjected = static args =>
    {
        Console.WriteLine("OnFaultInjected, Exception: {0}, Operation: {1}.", args.Fault.Message, args.Context.OperationKey);
        return default;
    }
};

// Add a fault strategy with a FaultStrategyOptions instance to the pipeline
new ResiliencePipelineBuilder().AddChaosFault(optionsBasic);
new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosFault(optionsWithFaultGenerator);

// There are also a couple of handy overloads to inject the chaos easily.
new ResiliencePipelineBuilder().AddChaosFault(0.1, () => new InvalidOperationException("Dummy exception"));
```
<!-- endSnippet -->

Example execution:

<!-- snippet: chaos-fault-execution -->
```cs
var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,  // Adds a random factor to the delay
        MaxRetryAttempts = 4,
        Delay = TimeSpan.FromSeconds(3),
    })
    .AddChaosFault(new FaultStrategyOptions // Chaos strategies are usually placed as the last ones in the pipeline
    {
        FaultGenerator = static args => new ValueTask<Exception?>(new InvalidOperationException("Dummy exception")),
        Enabled = true,
        InjectionRate = 0.1
    })
    .Build();
```
<!-- endSnippet -->

## Defaults

| Property          | Default Value | Description                                          |
|-------------------|---------------|------------------------------------------------------|
| `OnFaultInjected` | `null`        | Action executed when the fault is injected.          |
| `FaultGenerator`  | `null`        | Generates the fault to inject for a given execution. |

## Diagrams

### Normal 🐵 sequence diagram

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant F as Fault
    participant D as DecoratedUserCallback

    C->>P: Calls ExecuteAsync
    P->>F: Calls ExecuteCore
    activate F
    F-->>F: Determines Injection<br/>Decision: 🐵
    deactivate F
    F->>+D: Invokes
    D->>-F: Returns result
    F->>P: Returns result
    P->>C: Returns result
```

### Chaos 🙈 sequence diagram

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant F as Fault
    participant D as DecoratedUserCallback

    C->>P: Calls ExecuteAsync
    P->>F: Calls ExecuteCore
    activate F
    F-->>F: Determines Injection<br/>Decision: 🙈
    F-->>F: Injects Fault
    deactivate F
    Note over D: The user's Callback is not invoked<br/>when a fault is injected
    F->>P: Throws injected Fault
    P->>C: Propagates Exception
```
