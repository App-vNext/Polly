using Extensibility.Proactive;
using Extensibility.Reactive;
using Polly;
using System.Net;

#region ext-proactive-strategy-usage

// Add the proactive strategy to the builder
var pipeline = new ResiliencePipelineBuilder()
    // This is custom extension defined in this sample
    .AddTiming(new TimingStrategyOptions
    {
        Threshold = TimeSpan.FromSeconds(1),
        OnThresholdExceeded = args =>
        {
            Console.WriteLine("Execution threshold exceeded!");
            return default;
        },
    })
    .Build();

#endregion

#region ext-reactive-strategy-usage

// Add reactive strategy to the builder
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddResultReporting(new ResultReportingStrategyOptions<HttpResponseMessage>
    {
        // Define what outcomes to handle
        ShouldHandle = args => args.Outcome switch
        {
            { Exception: { } } => PredicateResult.True(),
            { Result: { StatusCode: HttpStatusCode.InternalServerError } } => PredicateResult.True(),
            _ => PredicateResult.False()
        },
        OnReportResult = args =>
        {
            Console.WriteLine($"Result: {args.Outcome}");
            return default;
        }
    });

// You can also use the non-generic ResiliencePipelineBuilder to handle any kind of result.
new ResiliencePipelineBuilder()
    .AddResultReporting(new ResultReportingStrategyOptions
    {
        // Define what outcomes to handle
        ShouldHandle = args => args.Outcome switch
        {
            { Exception: { } } => PredicateResult.True(),
            { Result: HttpResponseMessage message } when message.StatusCode == HttpStatusCode.InternalServerError => PredicateResult.True(),
            _ => PredicateResult.False()
        },
        OnReportResult = args =>
        {
            Console.WriteLine($"Result: {args.Outcome}");
            return default;
        }
    });

#endregion

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
