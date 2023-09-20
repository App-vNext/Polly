using Polly;

namespace Extensibility.Proactive;

#region ext-proactive-args

// Arguments-based structs encapsulate information about particular event that occurred inside resilience strategy.
// They cna expose any properties that are relevant to the event.
// For this event the actual duration of execution and the threshold that was exceeded are relevant.
public readonly struct ThresholdExceededArguments
{
    public ThresholdExceededArguments(ResilienceContext context, TimeSpan threshold, TimeSpan duration)
    {
        Context = context;
        Threshold = threshold;
        Duration = duration;
    }

    public TimeSpan Threshold { get; }

    public TimeSpan Duration { get; }

    // By convention, all arguments should expose the "Context" property.
    public ResilienceContext Context { get; }
}

#endregion
