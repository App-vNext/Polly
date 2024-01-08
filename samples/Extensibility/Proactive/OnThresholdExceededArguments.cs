using Polly;

namespace Extensibility.Proactive;

#region ext-proactive-args

// Structs for arguments encapsulate details about specific events within the resilience strategy.
// Relevant properties to the event can be exposed. In this event, the actual execution time and the exceeded threshold are included.
public readonly struct OnThresholdExceededArguments
{
    public OnThresholdExceededArguments(ResilienceContext context, TimeSpan threshold, TimeSpan duration)
    {
        Context = context;
        Threshold = threshold;
        Duration = duration;
    }

    public TimeSpan Threshold { get; }

    public TimeSpan Duration { get; }

    // As per convention, all arguments should provide a "Context" property.
    public ResilienceContext Context { get; }
}

#endregion
