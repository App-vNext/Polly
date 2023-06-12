using Polly.Hedging.Controller;

namespace Polly.Hedging.Utils;

internal sealed class HedgingController<T>
{
    private readonly ObjectPool<HedgingExecutionContext<T>> _contextPool;
    private readonly ObjectPool<TaskExecution<T>> _executionPool;
    private int _rentedContexts;
    private int _rentedExecutions;

    public HedgingController(
        TimeProvider provider,
        HedgingHandler<T> handler,
        int maxAttempts)
    {
        // retrieve the cancellation pool for this time provider
        var pool = CancellationTokenSourcePool.Create(provider);

        _executionPool = new ObjectPool<TaskExecution<T>>(() =>
        {
            Interlocked.Increment(ref _rentedExecutions);
            return new TaskExecution<T>(handler, pool);
        },
        _ =>
        {
            Interlocked.Decrement(ref _rentedExecutions);

            // Stryker disable once Boolean : no means to test this
            return true;
        });

        _contextPool = new ObjectPool<HedgingExecutionContext<T>>(
            () =>
            {
                Interlocked.Increment(ref _rentedContexts);
                return new HedgingExecutionContext<T>(_executionPool, provider, maxAttempts, ReturnContext);
            },
            _ =>
            {
                Interlocked.Decrement(ref _rentedContexts);

                // Stryker disable once Boolean : no means to test this
                return true;
            });
    }

    public int RentedContexts => _rentedContexts;

    public int RentedExecutions => _rentedExecutions;

    public HedgingExecutionContext<T> GetContext(ResilienceContext context)
    {
        var executionContext = _contextPool.Get();
        executionContext.Initialize(context);
        return executionContext;
    }

    private void ReturnContext(HedgingExecutionContext<T> context) => _contextPool.Return(context);
}
