# Hedging resilience strategy

## About

- **Options**: [`HedgingStrategyOptions<T>`](../../src/Polly.Core/Hedging/HedgingStrategyOptions.TResult.cs)
- **Extensions**: `AddHedging`
- **Strategy Type**: Reactive

🚧 This documentation is being written as part of the Polly v8 release.

## Usage

<!-- snippet: hedging -->
```cs
// Add hedging with default options.
// See https://github.com/App-vNext/Polly/blob/main/docs/strategies/hedging.md#defaults for default values.
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

| Property            | Default Value                                                               | Description                                                                              |
| ------------------- | --------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| `ShouldHandle`      | Predicate that handles all exceptions except `OperationCancelledException`. | Predicate that determines what results and exceptions are handled by the retry strategy. |
| `MaxHedgedAttempts` | 1                                                                           | The maximum number of hedged actions to use, in addition to the original action.         |
| `Delay`             | 2 seconds                                                                   | The maximum waiting time before spawning a new hedged action.                            |
| `ActionGenerator`   | Returns the original callback that was passed to the hedging strategy.      | Generator that creates hedged actions.                                                   |
| `DelayGenerator`    | `Null`                                                                      | Used for generating custom delays for hedging.                                           |
| `OnHedging`         | `Null`                                                                      | Event that is raised when a hedging is performed.                                        |
