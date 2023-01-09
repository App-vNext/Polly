using System.Diagnostics;
using Polly;

namespace Resilience;

public static partial class ResilienceStrategyExtensions
{
    public static void Execute<TState>(this IResilienceStrategy strategy, Action<TState> execute, TState state)
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

    public static T Execute<T>(this IResilienceStrategy strategy, Func<T> execute)
    {
        var context = ResilienceContext.Get();
        context.IsSynchronous = true;

        try
        {
            return strategy.ExecuteAsync(static (_, state) => new ValueTask<T>(state()), context, execute).GetResult();
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    public static T Execute<T, TState>(this IResilienceStrategy strategy, Func<TState, T> execute, TState state)
    {
        var context = ResilienceContext.Get();
        context.IsSynchronous = true;

        try
        {
            return strategy.ExecuteAsync(static (_, state) => new ValueTask<T>(state.execute(state.state)), context, (execute, state)).GetResult();
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    private static T GetResult<T>(this ValueTask<T> task)
    {
        Debug.Assert(
            task.IsCompleted,
            "The value task should be already completed at this point. If not, its and indication that the strategy does not respect the ResilienceContext.IsSynchronous value.");

#pragma warning disable S5034 // "ValueTask" should be consumed correctly
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits

        if (task.IsCompleted)
        {
            return task.Result;
        }

        return task.Preserve().Result;

#pragma warning restore S5034 // "ValueTask" should be consumed correctly
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }
}
