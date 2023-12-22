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
            { Result.StatusCode: HttpStatusCode.InternalServerError } => PredicateResult.True(),
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

