# Outcome chaos strategy

> [!IMPORTANT]
> This documentation page describes an upcoming feature of Polly.

## About

- **Options**:
  - [`OutcomeStrategyOptions`](xref:Polly.Simmy.Outcomes.OutcomeStrategyOptions)
  - [`OutcomeStrategyOptions<T>`](xref:Polly.Simmy.Outcomes.OutcomeStrategyOptions`1)
- **Extensions**: `AddChaosResult`
- **Strategy Type**: Reactive

---

The outcome chaos strategy is designed to inject or substitute fake results into system operations. This allows testing how an application behaves when it receives different types of responses, like successful results, errors, or exceptions.

## Usage

<!-- snippet: chaos-result-usage -->
```cs
// To use OutcomeGenerator<T> to register the results and exceptions to be injected (equal probability)
var optionsWithResultGenerator = new OutcomeStrategyOptions<HttpResponseMessage>
{
    OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>()
        .AddResult(() => new HttpResponseMessage(HttpStatusCode.TooManyRequests))
        .AddResult(() => new HttpResponseMessage(HttpStatusCode.InternalServerError))
        .AddException(() => new HttpRequestException("Chaos request exception.")),
    Enabled = true,
    InjectionRate = 0.1
};

// To get notifications when a result is injected
var optionsOnBehaviorInjected = new OutcomeStrategyOptions<HttpResponseMessage>
{
    OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>()
        .AddResult(() => new HttpResponseMessage(HttpStatusCode.InternalServerError)),
    Enabled = true,
    InjectionRate = 0.1,
    OnOutcomeInjected = static args =>
    {
        Console.WriteLine($"OnBehaviorInjected, Outcome: {args.Outcome.Result}, Operation: {args.Context.OperationKey}.");
        return default;
    }
};

// Add a result strategy with a OutcomeStrategyOptions{<TResult>} instance to the pipeline
new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosResult(optionsWithResultGenerator);
new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosResult(optionsOnBehaviorInjected);

// There are also a couple of handy overloads to inject the chaos easily
new ResiliencePipelineBuilder<HttpResponseMessage>().AddChaosResult(0.1, () => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
```
<!-- endSnippet -->

Example execution:

<!-- snippet: chaos-result-execution -->
```cs
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = static args => args.Outcome switch
        {
            { Result.StatusCode: HttpStatusCode.InternalServerError } => PredicateResult.True(),
            _ => PredicateResult.False()
        },
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        MaxRetryAttempts = 4,
        Delay = TimeSpan.FromSeconds(3),
    })
    .AddChaosResult(new OutcomeStrategyOptions<HttpResponseMessage> // Chaos strategies are usually placed as the last ones in the pipeline
    {
        OutcomeGenerator = static args =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            return new ValueTask<Outcome<HttpResponseMessage>?>(Outcome.FromResult(response));
        },
        Enabled = true,
        InjectionRate = 0.1
    })
    .Build();
```
<!-- endSnippet -->

## Defaults

| Property            | Default Value | Description                                             |
|---------------------|---------------|---------------------------------------------------------|
| `OutcomeGenerator`  | `null`        | Function to generate the outcome for a given execution. |
| `OnOutcomeInjected` | `null`        | Action executed when the outcome is injected.           |

## Diagrams

### Normal 🐵 sequence diagram

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant B as Outcome
    participant D as DecoratedUserCallback

    C->>P: Calls ExecuteAsync
    P->>B: Calls ExecuteCore
    activate B
    B-->>B: Determines Injection<br/>Decision: 🐵
    deactivate B
    B->>+D: Invokes
    D->>-B: Returns result
    B->>P: Returns result
    P->>C: Returns result
```

### Chaos 🙈 sequence diagram

```mermaid
sequenceDiagram
    actor C as Caller
    participant P as Pipeline
    participant B as Outcome
    participant D as DecoratedUserCallback

    C->>P: Calls ExecuteAsync
    P->>B: Calls ExecuteCore
    activate B
    B-->>B: Determines Injection<br/>Decision: 🙈
    B-->>B: Injects Outcome
    deactivate B
    Note over D: The user's Callback is not invoked<br/>when a fake result is injected
    B->>P: Returns result
    P->>C: Returns result
```

## Generating outcomes

To generate a faulted outcome (result or exception), you need to specify a `OutcomeGenerator` delegate. You have the following options as to how you customize this delegate:

### Use `OutcomeGenerator<T>` class to generate outcomes

The `OutcomeGenerator<T>` is a convenience API that allows you to specify what outcomes (results or exceptions) are to be injected. Additionally, it also allows assigning weight to each registered outcome.

<!-- snippet: chaos-outcome-generator-class -->
```cs
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddChaosResult(new OutcomeStrategyOptions<HttpResponseMessage>
    {
        // Use OutcomeGenerator<T> to register the results and exceptions to be injected
        OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>()
            .AddResult(() => new HttpResponseMessage(HttpStatusCode.InternalServerError)) // Result generator
            .AddResult(() => new HttpResponseMessage(HttpStatusCode.TooManyRequests), weight: 50) // Result generator with weight
            .AddResult(context => CreateResultFromContext(context)) // Access the ResilienceContext to create result
            .AddException<HttpRequestException>(), // You can also register exceptions
    });
```
<!-- endSnippet -->

### Use delegates to generate faults

Delegates give you the most flexibility at the expense of slightly more complicated syntax. Delegates also support asynchronous outcome generation, if you ever need that possibility.

<!-- snippet: chaos-outcome-generator-delegate -->
```cs
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddChaosResult(new OutcomeStrategyOptions<HttpResponseMessage>
    {
        // The same behavior can be achieved with delegates
        OutcomeGenerator = args =>
        {
            Outcome<HttpResponseMessage>? outcome = Random.Shared.Next(350) switch
            {
                < 100 => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
                < 150 => Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)),
                < 250 => Outcome.FromResult(CreateResultFromContext(args.Context)),
                < 350 => Outcome.FromException<HttpResponseMessage>(new TimeoutException()),
                _ => null
            };

            return ValueTask.FromResult(outcome);
        }
    });
```
<!-- endSnippet -->
