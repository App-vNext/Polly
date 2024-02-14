using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Polly.Utils.Pipeline;

/// <summary>
/// A component that delegates the execution to the next component in the chain.
/// </summary>
internal sealed class DelegatingComponent : PipelineComponent
{
    private readonly PipelineComponent _component;

    public DelegatingComponent(PipelineComponent component) => _component = component;

    public PipelineComponent? Next { get; set; }

    public override ValueTask DisposeAsync() => default;

    [ExcludeFromCodeCoverage]
    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
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

        return next.ExecuteCore(callback, context, state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask<Outcome<TResult>> ExecuteComponent<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
        => _component.ExecuteCore(
                static (context, state) => ExecuteNext(state.Next!, state.callback, context, state.state),
                context,
                (Next, callback, state));

#if NET6_0_OR_GREATER
    // Custom state object is used to cast the callback and state to prevent infinite
    // generic type recursion warning IL3054 when referenced in a native AoT application.
    // See https://github.com/App-vNext/Polly/issues/1732 for further context.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask<Outcome<TResult>> ExecuteComponentAot<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) =>
        _component.ExecuteCore(
            static (context, wrapper) =>
            {
                var callback = (Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>>)wrapper.Callback;
                var state = (TState)wrapper.State;
                return ExecuteNext(wrapper.Next, callback, context, state);
            },
            context,
            new StateWrapper(Next!, callback, state!));

    private readonly record struct StateWrapper(PipelineComponent Next, object Callback, object State);
#endif
}
