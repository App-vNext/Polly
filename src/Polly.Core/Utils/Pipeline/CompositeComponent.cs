using Polly.Telemetry;

namespace Polly.Utils.Pipeline;

/// <summary>
/// A combination of multiple components.
/// </summary>
[DebuggerDisplay("Pipeline, Strategies = {Strategies.Count}")]
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
        var delegatingComponents = components
            .Take(components.Count - 1)
            .Select(strategy => new DelegatingComponent(strategy))
            .ToList();

#if NET6_0_OR_GREATER
        // link the last one
        delegatingComponents[^1].Next = components[^1];
#else
        delegatingComponents[delegatingComponents.Count - 1].Next = components[components.Count - 1];
#endif

        // link the remaining ones
        for (var i = 0; i < delegatingComponents.Count - 1; i++)
        {
            delegatingComponents[i].Next = delegatingComponents[i + 1];
        }

        return new CompositeComponent(delegatingComponents[0], components, telemetry, timeProvider);
    }

    public IReadOnlyList<PipelineComponent> Components { get; }

    public override void Dispose()
    {
        foreach (var component in Components)
        {
            component.Dispose();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        foreach (var component in Components)
        {
            await component.DisposeAsync().ConfigureAwait(false);
        }
    }

    internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var timeStamp = _timeProvider.GetTimestamp();
        _telemetry.Report(new ResilienceEvent(ResilienceEventSeverity.Debug, TelemetryUtil.PipelineExecuting), context, default(PipelineExecutingArguments));

        Outcome<TResult> outcome;

        if (context.CancellationToken.IsCancellationRequested)
        {
            outcome = Outcome.FromException<TResult>(new OperationCanceledException(context.CancellationToken).TrySetStackTrace());
        }
        else
        {
            outcome = await FirstComponent.ExecuteCore(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        _telemetry.Report(
            new ResilienceEvent(ResilienceEventSeverity.Information, TelemetryUtil.PipelineExecuted),
            new OutcomeArguments<TResult, PipelineExecutedArguments>(context, outcome, new PipelineExecutedArguments(_timeProvider.GetElapsedTime(timeStamp))));

        return outcome;
    }

    /// <summary>
    /// A component that delegates the execution to the next component in the chain.
    /// </summary>
    private sealed class DelegatingComponent : PipelineComponent
    {
        private readonly PipelineComponent _component;

        public DelegatingComponent(PipelineComponent component) => _component = component;

        public PipelineComponent? Next { get; set; }

        internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            return _component.ExecuteCore(
                static (context, state) =>
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        return Outcome.FromExceptionAsTask<TResult>(new OperationCanceledException(context.CancellationToken).TrySetStackTrace());
                    }

                    return state.Next!.ExecuteCore(state.callback, context, state.state);
                },
                context,
                (Next, callback, state));
        }

        public override void Dispose()
        {
        }

        public override ValueTask DisposeAsync() => default;
    }
}
