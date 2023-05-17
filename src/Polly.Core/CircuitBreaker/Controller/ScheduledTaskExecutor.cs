using System.Threading;
using System.Threading.Tasks;

namespace Polly.CircuitBreaker;

#pragma warning disable CA1031 // Do not catch general exception types

/// <summary>
/// The scheduled task executor makes sure that tasks are executed in the order they were scheduled and not concurrently.
/// </summary>
internal sealed class ScheduledTaskExecutor : IDisposable
{
    private readonly ConcurrentQueue<Entry> _tasks = new();
    private readonly SemaphoreSlim _semaphore = new(0);
    private bool _disposed;

    public ScheduledTaskExecutor() => ProcessingTask = Task.Run(StartProcessingAsync);

    public Task ProcessingTask { get; }

    public void ScheduleTask(Func<Task> taskFactory, ResilienceContext context, out Task task)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ScheduledTaskExecutor));
        }

        var source = new TaskCompletionSource<object>();
        task = source.Task;

        _tasks.Enqueue(new Entry(taskFactory, context.ContinueOnCapturedContext, source));
        _semaphore.Release();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _semaphore.Release();

        while (_tasks.TryDequeue(out var e))
        {
            e.TaskCompletion.TrySetCanceled();
        }

        _semaphore.Dispose();
    }

    private async Task StartProcessingAsync()
    {
        while (true)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            if (_disposed)
            {
                return;
            }

            _ = _tasks.TryDequeue(out var entry);

            try
            {
                await entry!.TaskFactory().ConfigureAwait(entry.ContinueOnCapturedContext);
                entry.TaskCompletion.SetResult(null!);
            }
            catch (OperationCanceledException)
            {
                entry!.TaskCompletion.SetCanceled();
            }
            catch (Exception e)
            {
                entry!.TaskCompletion.SetException(e);
            }

            if (_disposed)
            {
                return;
            }
        }
    }

    private record Entry(Func<Task> TaskFactory, bool ContinueOnCapturedContext, TaskCompletionSource<object> TaskCompletion);
}
