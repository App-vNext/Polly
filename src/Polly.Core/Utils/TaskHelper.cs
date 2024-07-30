using System.Runtime.CompilerServices;

namespace Polly.Utils;

#pragma warning disable S5034 // "ValueTask" should be consumed correctly

internal static class TaskHelper
{
    public static void GetResult(this ValueTask<VoidResult> task)
    {
        Debug.Assert(
            task.IsCompleted,
            "The value task should be already completed at this point. If not, it's an indication that the strategy does not respect the ResilienceContext.IsSynchronous value.");

        // Stryker disable once boolean : no means to test this
        if (task.IsCompleted)
        {
            _ = task.Result;
            return;
        }

        task.Preserve().GetAwaiter().GetResult();
    }

    public static TResult GetResult<TResult>(this ValueTask<TResult> task)
    {
        Debug.Assert(
            task.IsCompleted,
            "The value task should be already completed at this point. If not, it's an indication that the strategy does not respect the ResilienceContext.IsSynchronous value.");

        // Stryker disable once boolean : no means to test this
        if (task.IsCompleted)
        {
            return task.Result;
        }

        return task.Preserve().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Convert a <see cref="ValueTask{T}"/> to a <see cref="ValueTask{Outcome{T}}"/>, transfering the potential
    /// exceptions into the <see cref="Outcome{T}"/> to avoid rethrowing them when the task is awaited.
    /// </summary>
    /// <param name="task">Task to convert.</param>
    /// <typeparam name="T">Outcome value.</typeparam>
    /// <returns>An successful or unsuccessful outcome.</returns>
    public static ValueTask<Outcome<T>> ToAsyncOutcome<T>(this ValueTask<T> task) => ToAsyncOutcomeAwaiter<T>.GetTaskOutcome(task);

    private readonly struct ToAsyncOutcomeAwaiter<T>
    {
        private readonly ValueTask<T> _task;
        private readonly TaskCompletionSource<Outcome<T>> _tcs;

        private ValueTask<Outcome<T>> TaskResult => new(_tcs.Task);

        public static ValueTask<Outcome<T>> GetTaskOutcome(ValueTask<T> task) =>
            new ToAsyncOutcomeAwaiter<T>(task).TaskResult;

        private ToAsyncOutcomeAwaiter(ValueTask<T> task)
        {
            _task = task;
            _tcs = new TaskCompletionSource<Outcome<T>>();

            // Setup task completed callback
            ValueTaskAwaiter<T> awaiter = _task.GetAwaiter();
            awaiter.OnCompleted(OnTaskCompleted);
        }

        private void OnTaskCompleted()
        {
            if (_task.IsCompletedSuccessfully)
            {
                _tcs.TrySetResult(Outcome.FromResult<T>(_task.Result));
            }
            else
            {
                _tcs.TrySetResult(Outcome.FromException<T>(_task.IsCanceled ? new TaskCanceledException() : _task.AsTask().Exception!.Unwrap()));
            }
        }
    }

    private static Exception Unwrap(this Exception exception)
    {
        if (exception is AggregateException aggregateException
            && aggregateException.InnerExceptions.Count == 1
            && aggregateException.InnerException != null)
        {
            return aggregateException.InnerException;
        }

        return exception;
    }
}
