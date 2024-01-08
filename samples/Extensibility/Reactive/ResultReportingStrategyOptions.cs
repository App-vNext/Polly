using Polly;
using System.ComponentModel.DataAnnotations;

namespace Extensibility.Reactive;

#region ext-reactive-options

public class ResultReportingStrategyOptions<TResult> : ResilienceStrategyOptions
{
    public ResultReportingStrategyOptions()
    {
        // Assign a default name to the options for more detailed telemetry insights.
        Name = "ResultReporting";
    }

    // Options for reactive strategies should always include a "ShouldHandle" delegate.
    // Set a sensible default when possible. Here, we handle all exceptions.
    public Func<ResultReportingPredicateArguments<TResult>, ValueTask<bool>> ShouldHandle { get; set; } = args =>
    {
        return new ValueTask<bool>(args.Outcome.Exception is not null);
    };

    // This illustrates an event delegate. Note that the arguments struct carries the same name as the delegate but with an "Arguments" suffix.
    // The event follows the async convention and must be set by the user.
    //
    // The [Required] attribute enforces the consumer to specify this property, used when some properties do not have sensible defaults and are required.
    [Required]
    public Func<OnReportResultArguments<TResult>, ValueTask>? OnReportResult { get; set; }
}

#endregion

#region ext-reactive-non-generic-options

// Simply derive from the generic options, using 'object' as the result type.
// This allows the strategy to manage all results.
public class ResultReportingStrategyOptions : ResultReportingStrategyOptions<object>
{
}


#endregion
