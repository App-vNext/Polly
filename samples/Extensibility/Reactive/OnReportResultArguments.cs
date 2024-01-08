using Polly;

namespace Extensibility.Reactive;

#region ext-reactive-event-args

public readonly struct OnReportResultArguments<TResult>
{
    public OnReportResultArguments(ResilienceContext context, Outcome<TResult> outcome)
    {
        Context = context;
        Outcome = outcome;
    }

    // Always include the "Context" property in the arguments.
    public ResilienceContext Context { get; }

    // Always have the "Outcome" property in reactive arguments.
    public Outcome<TResult> Outcome { get; }
}

#endregion
