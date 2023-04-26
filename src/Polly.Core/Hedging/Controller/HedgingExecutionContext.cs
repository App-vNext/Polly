using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly.Hedging.Controller;
using Polly.Strategy;
using Polly.Utils;
using static Polly.Hedging.Utils.HedgingExecutionContext;

namespace Polly.Hedging.Utils;

#pragma warning disable CA1031 // Do not catch general exception types

/// <summary>
/// The context associated with an execution of hedging resilience strategy.
/// It holds the resources for all executed hedged tasks (primary + secondary) and is responsible for resource disposal.
/// </summary>
internal sealed class HedgingExecutionContext
{
    public readonly record struct ExecutionInfo<TResult>(TaskExecution? Execution, bool Loaded, Outcome<TResult>? Outcome);

    private readonly List<TaskExecution> _tasks = new();
    private readonly List<TaskExecution> _executingTasks = new();
    private readonly IObjectPool<TaskExecution> _executionPool;
    private readonly TimeProvider _timeProvider;
    private readonly int _maxAttempts;
    private readonly Action<HedgingExecutionContext> _onReset;
    private readonly ResilienceProperties _replacedProperties = new();

    public HedgingExecutionContext(IObjectPool<TaskExecution> executionPool, TimeProvider timeProvider, int maxAttempts, Action<HedgingExecutionContext> onReset)
    {
        _executionPool = executionPool;
        _timeProvider = timeProvider;
        _maxAttempts = maxAttempts;
        _onReset = onReset;
    }

    internal void Initialize(ResilienceContext context)
    {
        Snapshot = new ContextSnapshot(context, context.Properties, context.CancellationToken);
        _replacedProperties.Replace(Snapshot.OriginalProperties);
        Snapshot.Context.Properties = _replacedProperties;
    }

    public int LoadedTasks => _tasks.Count;

    public ContextSnapshot Snapshot { get; private set; }

    public bool IsInitialized => Snapshot.Context != null;

    public IReadOnlyList<TaskExecution> Tasks => _tasks;

    private bool ContinueOnCapturedContext => Snapshot.Context.ContinueOnCapturedContext;

    public async ValueTask<ExecutionInfo<TResult>> LoadExecutionAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> primaryCallback, TState state)
    {
        if (LoadedTasks >= _maxAttempts)
        {
            return CreateExecutionInfoWhenNoExecution<TResult>();
        }

        // determine what type of task we are creating
        var type = LoadedTasks switch
        {
            0 => HedgedTaskType.Primary,
            _ => HedgedTaskType.Secondary
        };

        var execution = _executionPool.Get();

        if (await execution.InitializeAsync(type, Snapshot, primaryCallback, state, LoadedTasks).ConfigureAwait(ContinueOnCapturedContext))
        {
            // we were able to start a new execution, register it
            _tasks.Add(execution);
            _executingTasks.Add(execution);
            return new ExecutionInfo<TResult>(execution, true, null);
        }
        else
        {
            _executionPool.Return(execution);
            return CreateExecutionInfoWhenNoExecution<TResult>();
        }
    }

    public void Complete()
    {
        if (LoadedTasks == 0)
        {
            throw new InvalidOperationException("The hedging must execute at least one task.");
        }

        UpdateOriginalContext();

        // first, cancel any pending tasks
        foreach (var pair in _executingTasks)
        {
            pair.Cancel();
        }

        // We are intentionally doing the cleanup in the background as we do not want to
        // delay the hedging.
        // The background cleanup is safe. All exceptions are handled.
        _ = CleanupInBackgroundAsync();
    }

    public async ValueTask<TaskExecution?> TryWaitForCompletedExecutionAsync(TimeSpan hedgingDelay)
    {
        // before doing anything expensive, let's check whether any existing task is already completed
        if (TryRemoveExecutedTask() is TaskExecution execution)
        {
            return execution;
        }

        if (LoadedTasks == _maxAttempts)
        {
            await WaitForTaskCompetitionAsync().ConfigureAwait(ContinueOnCapturedContext);
            return TryRemoveExecutedTask();
        }

        if (hedgingDelay == TimeSpan.Zero || LoadedTasks == 0)
        {
            // just load the next task
            return null;
        }

        // Stryker disable once equality : no means to test this, stryker changes '<' to '<=' where 0 is already covered in the branch above
        if (hedgingDelay < TimeSpan.Zero)
        {
            await WaitForTaskCompetitionAsync().ConfigureAwait(ContinueOnCapturedContext);
            return TryRemoveExecutedTask();
        }

        using var delayTaskCancellation = CancellationTokenSource.CreateLinkedTokenSource(Snapshot.Context.CancellationToken);

        var delayTask = _timeProvider.DelayAsync(hedgingDelay, Snapshot.Context);
        Task<Task> whenAnyHedgedTask = WaitForTaskCompetitionAsync();
        var completedTask = await Task.WhenAny(whenAnyHedgedTask, delayTask).ConfigureAwait(ContinueOnCapturedContext);

        if (completedTask == delayTask)
        {
            return null;
        }

        // cancel the ongoing delay task
        // Stryker disable once boolean : no means to test this
        delayTaskCancellation.Cancel(throwOnFirstException: false);
        await whenAnyHedgedTask.ConfigureAwait(ContinueOnCapturedContext);

        return TryRemoveExecutedTask();
    }

    private ExecutionInfo<TResult> CreateExecutionInfoWhenNoExecution<TResult>()
    {
        // if there are no more executing tasks we need to check finished ones
        if (_executingTasks.Count == 0)
        {
            var finishedExecution = _tasks.First(static t => t.ExecutionTask!.IsCompleted);
            finishedExecution.AcceptOutcome();
            return new ExecutionInfo<TResult>(null, false, finishedExecution.Outcome.AsOutcome<TResult>());
        }

        return new ExecutionInfo<TResult>(null, false, null);
    }

    private Task<Task> WaitForTaskCompetitionAsync()
    {
#pragma warning disable S109 // Magic numbers should not be used
        return _executingTasks.Count switch
        {
            1 => AwaitTask(_executingTasks[0], ContinueOnCapturedContext),
            2 => Task.WhenAny(_executingTasks[0].ExecutionTask!, _executingTasks[1].ExecutionTask!),
            _ => Task.WhenAny(_executingTasks.Select(v => v.ExecutionTask!))
        };
#pragma warning restore S109 // Magic numbers should not be used

        static async Task<Task> AwaitTask(TaskExecution task, bool continueOnCapturedContext)
        {
            // ExecutionTask never fails
            await task.ExecutionTask!.ConfigureAwait(continueOnCapturedContext);
            return Task.FromResult(task);
        }
    }

    private TaskExecution? TryRemoveExecutedTask()
    {
        int? index = null;

        for (var i = 0; i < _executingTasks.Count; i++)
        {
            var task = _executingTasks[i];
            if (task.ExecutionTask!.IsCompleted)
            {
                index = i;
                break;
            }
        }

        if (index is not null)
        {
            var execution = _executingTasks[index.Value];
            _executingTasks.RemoveAt(index.Value);
            return execution;
        }

        return null;
    }

    private void UpdateOriginalContext()
    {
        var originalContext = Snapshot.Context;
        originalContext.CancellationToken = Snapshot.OriginalCancellationToken;
        originalContext.Properties = Snapshot.OriginalProperties;

        int accepted = 0;
        TaskExecution? acceptedExecution = null;

        foreach (var task in Tasks)
        {
            if (task.IsAccepted)
            {
                accepted++;
                acceptedExecution = task;
            }
        }

        if (accepted != 1)
        {
            throw new InvalidOperationException($"There must be exactly one accepted outcome for hedging. Found {accepted}.");
        }

        originalContext.Properties.Replace(acceptedExecution!.Properties);

        if (acceptedExecution.Type == HedgedTaskType.Secondary)
        {
            foreach (var @event in acceptedExecution.Context.ResilienceEvents)
            {
                originalContext.AddResilienceEvent(@event);
            }
        }
    }

    private async Task CleanupInBackgroundAsync()
    {
        foreach (var task in _tasks)
        {
            await task.ExecutionTask!.ConfigureAwait(false);
            await task.ResetAsync().ConfigureAwait(false);
            _executionPool.Return(task);
        }

        Reset();
    }

    private void Reset()
    {
        _replacedProperties.Clear();
        _tasks.Clear();

        _executingTasks.Clear();
        Snapshot = default;

        _onReset(this);
    }
}
