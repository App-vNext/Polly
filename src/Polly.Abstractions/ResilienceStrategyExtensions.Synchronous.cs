using System.Diagnostics;
using Polly;

namespace Resilience;

public static partial class ResilienceStrategyExtensions
{
    public static void Execute<TState>(
        this IResilienceStrategy strategy,
        Action<TState> execute,
        TState state)
    {
        var context = ResilienceContext.Get();
        context.IsSynchronous = true;
        context.IsVoid = true;

        try
        {
            _ = strategy.ExecuteAsync(
                static (_, state) =>
                {
                    state.execute(state.state);
                    return new ValueTask<VoidResult>(VoidResult.Instance);
                },
                context,
                (execute, state)).GetResult();
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    public static void Execute(this IResilienceStrategy strategy, Action execute)
    {
        var context = ResilienceContext.Get();
        context.IsSynchronous = true;
        context.IsVoid = true;

        try
        {
            _ = strategy.ExecuteAsync(
                static (_, state) =>
                {
                    state();
                    return new ValueTask<VoidResult>(VoidResult.Instance);
                },
                context,
                execute).GetResult();
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    public static TResult Execute<TResult>(this IResilienceStrategy strategy, Func<TResult> execute)
    {
        var context = ResilienceContext.Get();
        context.IsSynchronous = true;

        try
        {
            return strategy.ExecuteAsync(static (_, state) => new ValueTask<TResult>(state()), context, execute).GetResult();
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    public static TResult Execute<TResult, TState>(
        this IResilienceStrategy strategy,
        Func<TState, TResult> execute,
        TState state)
    {
        var context = ResilienceContext.Get();
        context.IsSynchronous = true;

        try
        {
            return strategy.ExecuteAsync(static (_, state) => new ValueTask<TResult>(state.execute(state.state)), context, (execute, state)).GetResult();
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    private static TResult GetResult<TResult>(this ValueTask<TResult> task)
    {
        Debug.Assert(
            task.IsCompleted,
            "The value task should be already completed at this point. If not, it's an indication that the strategy does not respect the ResilienceContext.IsSynchronous value.");

        if (task.IsCompleted)
        {
            return task.Result;
        }

        return task.Preserve().Result;
    }
}
