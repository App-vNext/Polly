using Extensibility.Proactive;
using Polly;
using System.Net.Http.Headers;

// ------------------------------------------------------------------------
// Usage of custom proactive strategy
// ------------------------------------------------------------------------
var pipeline = new ResiliencePipelineBuilder()
    // This is custom extension defined in this sample
    .AddTiming(new TimingStrategyOptions
    {
        Threshold = TimeSpan.FromSeconds(1),
        ThresholdExceeded = args =>
        {
            Console.WriteLine("Execution threshold exceeded!");
            return default;
        },
    })
    .Build();

// Execute the pipeline
await pipeline.ExecuteAsync(async token => await Task.Delay(1500, token), CancellationToken.None);

// ------------------------------------------------------------------------
// SIMPLE EXTENSIBILITY MODEL (INLINE STRATEGY)
// ------------------------------------------------------------------------
pipeline = new ResiliencePipelineBuilder()
    // Just add the strategy instance directly
    .AddStrategy(_ => new MySimpleStrategy(), new MySimpleStrategyOptions())
    .Build();

// Execute the pipeline
pipeline.Execute(() => { });

internal class MySimpleStrategy : ResilienceStrategy
{
    protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        Console.WriteLine("MySimpleStrategy executing!");

        // The "state" is an ambient value passed by the caller that holds the state.
        // Here, we do not do anything with it, just pass it to the callback.

        // Execute the provided callback
        return callback(context, state);
    }
}

public class MySimpleStrategyOptions : ResilienceStrategyOptions
{
}
