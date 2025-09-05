using Polly.Telemetry;

namespace Polly.Utils.Pipeline;

/// <summary>
/// A combination of multiple components.
/// </summary>
[DebuggerDisplay("Pipeline, Strategies = {Components.Count}")]
[DebuggerTypeProxy(typeof(CompositeComponentDebuggerProxy))]
internal sealed class CompositeComponent : PipelineComponent
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly TimeProvider _timeProvider;

    private CompositeComponent(
        PipelineComponent first,
        IReadOnlyList<PipelineComponent> components,
        ResilienceStrategyTelemetry telemetry,
        TimeProvider timeProvider)
    {
        Components = components;

        _telemetry = telemetry;
        _timeProvider = timeProvider;
        FirstComponent = first;
    }

    internal PipelineComponent FirstComponent { get; }

    public static PipelineComponent Create(
        IReadOnlyList<PipelineComponent> components,
        ResilienceStrategyTelemetry telemetry,
        TimeProvider timeProvider)
    {
        if (components.Count == 1)
        {
            return new CompositeComponent(components[0], components, telemetry, timeProvider);
        }

        // convert all components to delegating ones (except the last one as it's not required)
        DelegatingComponent[] delegatingComponents = [.. components
            .Take(components.Count - 1)
            .Select(static strategy => new DelegatingComponent(strategy))];

#if NET6_0_OR_GREATER
        // link the last one
        delegatingComponents[^1].Next = components[^1];
#else
        delegatingComponents[delegatingComponents.Length - 1].Next = components[components.Count - 1];
#endif

        // link the remaining ones
        for (var i = 0; i < delegatingComponents.Length - 1; i++)
        {
            delegatingComponents[i].Next = delegatingComponents[i + 1];
        }

        return new CompositeComponent(delegatingComponents[0], components, telemetry, timeProvider);
    }

    public IReadOnlyList<PipelineComponent> Components { get; }

    public override async ValueTask DisposeAsync()
    {
        foreach (var component in Components)
        {
            await component.DisposeAsync().ConfigureAwait(false);
        }
    }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (!_telemetry.Enabled)
        {
            return ExecuteCoreWithoutTelemetry(callback, context, state);
        }

        return ExecuteCoreWithTelemetry(callback, context, state);
    }

    private ValueTask<Outcome<TResult>> ExecuteCoreWithoutTelemetry<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (context.CancellationToken.IsCancellationRequested)
        {
            return Outcome.FromExceptionAsValueTask<TResult>(new OperationCanceledException(context.CancellationToken).TrySetStackTrace());
        }
        else
        {
            return FirstComponent.ExecuteCore(callback, context, state);
        }
    }

    private async ValueTask<Outcome<TResult>> ExecuteCoreWithTelemetry<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var timeStamp = _timeProvider.GetTimestamp();

        _telemetry.Report(new ResilienceEvent(ResilienceEventSeverity.Debug, TelemetryUtil.PipelineExecuting), context, default(PipelineExecutingArguments));

        var outcome = await ExecuteCoreWithoutTelemetry(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        _telemetry.Report(
            new ResilienceEvent(ResilienceEventSeverity.Information, TelemetryUtil.PipelineExecuted),
            context,
            outcome,
            new PipelineExecutedArguments(_timeProvider.GetElapsedTime(timeStamp)));

        return outcome;
    }
}
