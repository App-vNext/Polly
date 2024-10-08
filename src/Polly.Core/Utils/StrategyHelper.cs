namespace Polly.Utils;

#pragma warning disable CA1031 // Do not catch general exception types

internal static class StrategyHelper
{
    [DebuggerDisableUserUnhandledExceptions]
    public static ValueTask<Outcome<TResult>> ExecuteCallbackSafeAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (context.CancellationToken.IsCancellationRequested)
        {
            return new ValueTask<Outcome<TResult>>(Outcome.FromException<TResult>(new OperationCanceledException(context.CancellationToken)));
        }

        try
        {
            var callbackTask = callback(context, state);
            if (callbackTask.IsCompleted)
            {
                return new ValueTask<Outcome<TResult>>(callbackTask.GetResult());
            }

            return AwaitTask(callbackTask, context.ContinueOnCapturedContext);
        }
        catch (Exception e)
        {
            return new ValueTask<Outcome<TResult>>(Outcome.FromException<TResult>(e));
        }

        [DebuggerDisableUserUnhandledExceptions]
        static async ValueTask<Outcome<T>> AwaitTask<T>(ValueTask<Outcome<T>> task, bool continueOnCapturedContext)
        {
            try
            {
                return await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception e)
            {
                return Outcome.FromException<T>(e);
            }
        }
    }
}
