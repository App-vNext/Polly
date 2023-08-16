namespace Polly.Utils;

internal abstract partial class PipelineComponent
{
    [DebuggerDisplay("{Strategy}")]
    internal sealed class BridgeComponent<T> : BridgeComponentBase
    {
        public BridgeComponent(ResilienceStrategy<T> strategy)
            : base(strategy) => Strategy = strategy;

        public ResilienceStrategy<T> Strategy { get; }

        internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            // Check if we can cast directly, thus saving some cycles and improving the performance
            if (callback is Func<ResilienceContext, TState, ValueTask<Outcome<T>>> casted)
            {
                return TaskHelper.ConvertValueTask<T, TResult>(
                    Strategy.ExecuteCore(casted, context, state),
                    context);
            }
            else
            {
                var valueTask = Strategy.ExecuteCore(
                    static async (context, state) =>
                    {
                        var outcome = await state.callback(context, state.state).ConfigureAwait(context.ContinueOnCapturedContext);
                        return outcome.AsOutcome<T>();
                    },
                    context,
                    (callback, state));

                return TaskHelper.ConvertValueTask<T, TResult>(valueTask, context);
            }
        }
    }

    [DebuggerDisplay("{Strategy}")]
    internal sealed class BridgeComponent : BridgeComponentBase
    {
        public BridgeComponent(ResilienceStrategy strategy)
            : base(strategy) => Strategy = strategy;

        public ResilienceStrategy Strategy { get; }

        internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state) => Strategy.ExecuteCore(callback, context, state);
    }

    internal abstract class BridgeComponentBase : PipelineComponent
    {
        private readonly object _strategy;

        protected BridgeComponentBase(object strategy) => _strategy = strategy;

        public override void Dispose()
        {
            if (_strategy is IDisposable disposable)
            {
                disposable.Dispose();
            }
            else if (_strategy is IAsyncDisposable asyncDisposable)
            {
                asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
        }

        public override ValueTask DisposeAsync()
        {
            if (_strategy is IAsyncDisposable asyncDisposable)
            {
                return asyncDisposable.DisposeAsync();
            }
            else
            {
                Dispose();
                return default;
            }
        }
    }

}
