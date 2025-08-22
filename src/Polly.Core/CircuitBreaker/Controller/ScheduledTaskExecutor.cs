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

    public Task ScheduleTask(Func<Task> taskFactory)
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(_disposed, this);
#else
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ScheduledTaskExecutor));
        }
#endif

        var source = new TaskCompletionSource<object>();

        _tasks.Enqueue(new Entry(taskFactory, source));
        _semaphore.Release();
        return source.Task;
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
        while (!_disposed)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            if (_disposed || !_tasks.TryDequeue(out var entry))
            {
                return;
            }

            try
            {
                await entry.TaskFactory().ConfigureAwait(false);
                entry.TaskCompletion.TrySetResult(null!);
            }
            catch (OperationCanceledException)
            {
                entry.TaskCompletion.TrySetCanceled();
            }
            catch (Exception e)
            {
                entry.TaskCompletion.TrySetException(e);
            }
        }
    }

    private sealed record Entry(Func<Task> TaskFactory, TaskCompletionSource<object> TaskCompletion);
}
