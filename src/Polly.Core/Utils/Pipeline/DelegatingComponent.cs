using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Polly.Utils.Pipeline;

/// <summary>
/// A component that delegates the execution to the next component in the chain.
/// </summary>
internal sealed class DelegatingComponent : PipelineComponent
{
    private readonly PipelineComponent _component;
    private readonly ResilienceStrategy? _strategy;

    public DelegatingComponent(PipelineComponent component)
    {
        _component = component;

        // Non-generic strategies are wrapped in a trivial bridge component. Unwrap them here so that
        // the execution can skip the bridge and its generic virtual method dispatch.
        _strategy = (component as BridgeComponent)?.Strategy;
    }

    public PipelineComponent? Next { get; set; }

    public override ValueTask DisposeAsync() => default;

    [ExcludeFromCodeCoverage]
    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) => Execute(callback, context, state);

    /// <summary>
    /// Non-virtual equivalent of <see cref="ExecuteCore{TResult, TState}"/>.
    /// </summary>
    /// <remarks>
    /// Generic virtual method calls are dispatched through a runtime helper and are considerably more
    /// expensive than regular calls. Callers that statically know they are dealing with a
    /// <see cref="DelegatingComponent"/> (the composite root and the previous link in the chain)
    /// use this method to bypass that dispatch.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask<Outcome<TResult>> Execute<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) =>
#if NET6_0_OR_GREATER
        RuntimeFeature.IsDynamicCodeSupported ? ExecuteComponent(callback, context, state) : ExecuteComponentAot(callback, context, state);
#else
        ExecuteComponent(callback, context, state);
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Outcome<TResult>> ExecuteNext<TResult, TState>(
        PipelineComponent next,
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (context.CancellationToken.IsCancellationRequested)
        {
            return Outcome.FromExceptionAsValueTask<TResult>(new OperationCanceledException(context.CancellationToken).TrySetStackTrace());
        }

        // All components except the last one are delegating components, so avoid the
        // generic virtual method dispatch for them by calling the non-virtual method directly.
        if (next is DelegatingComponent delegating)
        {
            return delegating.Execute(callback, context, state);
        }

        // The last component is commonly a bridge to a non-generic strategy, skip the bridge.
        if (next is BridgeComponent bridge)
        {
            return bridge.Strategy.ExecuteCore(callback, context, state);
        }

        return next.ExecuteCore(callback, context, state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask<Outcome<TResult>> ExecuteComponent<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (_strategy is not null)
        {
            return _strategy.ExecuteCore(
                static (context, state) => ExecuteNext(state.Next!, state.callback, context, state.state),
                context,
                (Next, callback, state));
        }

        return _component.ExecuteCore(
            static (context, state) => ExecuteNext(state.Next!, state.callback, context, state.state),
            context,
            (Next, callback, state));
    }

#if NET6_0_OR_GREATER
    // Custom state object is used to cast the callback and state to prevent infinite
    // generic type recursion warning IL3054 when referenced in a native AoT application.
    // See https://github.com/App-vNext/Polly/issues/1732 for further context.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask<Outcome<TResult>> ExecuteComponentAot<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (_strategy is not null)
        {
            return _strategy.ExecuteCore(
                static (context, wrapper) =>
                {
                    var callback = (Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>>)wrapper.Callback;
                    var state = (TState)wrapper.State;
                    return ExecuteNext(wrapper.Next, callback, context, state);
                },
                context,
                new StateWrapper(Next!, callback, state!));
        }

        return _component.ExecuteCore(
            static (context, wrapper) =>
            {
                var callback = (Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>>)wrapper.Callback;
                var state = (TState)wrapper.State;
                return ExecuteNext(wrapper.Next, callback, context, state);
            },
            context,
            new StateWrapper(Next!, callback, state!));
    }

    private readonly record struct StateWrapper(PipelineComponent Next, object Callback, object State);
#endif
}
