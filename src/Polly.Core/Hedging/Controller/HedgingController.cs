using Polly.Hedging.Controller;
using Polly.Utils;

namespace Polly.Hedging.Utils;

internal sealed class HedgingController
{
    private readonly IObjectPool<HedgingExecutionContext> _contextPool;
    private readonly IObjectPool<TaskExecution> _executionPool;
    private int _rentedContexts;
    private int _rentedExecutions;

    public HedgingController(TimeProvider provider, HedgingHandler.Handler handler, int maxAttempts)
    {
        // retrieve the cancellation pool for this time provider
        var pool = CancellationTokenSourcePool.Create(provider);

        _executionPool = new ObjectPool<TaskExecution>(() =>
        {
            Interlocked.Increment(ref _rentedExecutions);
            return new TaskExecution(handler, pool);
        },
        _ =>
        {
            Interlocked.Decrement(ref _rentedExecutions);

            // Stryker disable once Boolean : no means to test this
            return true;
        });

        _contextPool = new ObjectPool<HedgingExecutionContext>(
            () =>
            {
                Interlocked.Increment(ref _rentedContexts);
                return new HedgingExecutionContext(_executionPool, provider, maxAttempts, ReturnContext);
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

    public HedgingExecutionContext GetContext(ResilienceContext context)
    {
        var executionContext = _contextPool.Get();
        executionContext.Initialize(context);
        return executionContext;
    }

    private void ReturnContext(HedgingExecutionContext context) => _contextPool.Return(context);
}
