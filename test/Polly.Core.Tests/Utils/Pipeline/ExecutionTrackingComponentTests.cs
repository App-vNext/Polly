using Microsoft.Extensions.Time.Testing;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Utils.Pipeline;

public class ExecutionTrackingComponentTests
{
    private readonly FakeTimeProvider _timeProvider = new();

    [Fact]
    public async Task DisposeAsync_PendingOperations_Delayed()
    {
        using var assert = new ManualResetEvent(false);
        using var executing = new ManualResetEvent(false);

        await using var inner = new Inner
        {
            OnExecute = () =>
            {
                executing.Set();
                assert.WaitOne();
            }
        };

        var component = new ExecutionTrackingComponent(inner, _timeProvider);
        var execution = Task.Run(() => new ResiliencePipeline(component, Polly.Utils.DisposeBehavior.Allow, null).Execute(() => { }));
        executing.WaitOne();

        var disposeTask = component.DisposeAsync().AsTask();
        _timeProvider.Advance(ExecutionTrackingComponent.SleepDelay);
        inner.Disposed.ShouldBeFalse();
        assert.Set();

        _timeProvider.Advance(ExecutionTrackingComponent.SleepDelay);
        await execution;

        _timeProvider.Advance(ExecutionTrackingComponent.SleepDelay);
        await disposeTask;

        inner.Disposed.ShouldBeTrue();
    }

    [Fact]
    public async Task HasPendingExecutions_Ok()
    {
        using var assert = new ManualResetEvent(false);
        using var executing = new ManualResetEvent(false);

        await using var inner = new Inner
        {
            OnExecute = () =>
            {
                executing.Set();
                assert.WaitOne();
            }
        };

        await using var component = new ExecutionTrackingComponent(inner, _timeProvider);
        var execution = Task.Run(() => new ResiliencePipeline(component, Polly.Utils.DisposeBehavior.Allow, null).Execute(() => { }));
        executing.WaitOne();

        component.HasPendingExecutions.ShouldBeTrue();
        assert.Set();
        await execution;

        component.HasPendingExecutions.ShouldBeFalse();
    }

    [Fact]
    public async Task DisposeAsync_Timeout_Ok()
    {
        using var assert = new ManualResetEvent(false);
        using var executing = new ManualResetEvent(false);

        await using var inner = new Inner
        {
            OnExecute = () =>
            {
                executing.Set();
                assert.WaitOne();
            }
        };

        var component = new ExecutionTrackingComponent(inner, _timeProvider);
        var execution = Task.Run(() => new ResiliencePipeline(component, Polly.Utils.DisposeBehavior.Allow, null).Execute(() => { }));
        executing.WaitOne();

        var disposeTask = component.DisposeAsync().AsTask();
        inner.Disposed.ShouldBeFalse();
        _timeProvider.Advance(ExecutionTrackingComponent.Timeout - TimeSpan.FromSeconds(1));
        inner.Disposed.ShouldBeFalse();
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        await disposeTask;
        inner.Disposed.ShouldBeTrue();

        assert.Set();
        await execution;
    }

    [Fact]
    public async Task DisposeAsync_WhenRunningMultipleTasks_Ok()
    {
        var tasks = new ConcurrentQueue<ManualResetEvent>();
        await using var inner = new Inner
        {
            OnExecute = () =>
            {
                var ev = new ManualResetEvent(false);
                tasks.Enqueue(ev);
                ev.WaitOne();
            }
        };

        var component = new ExecutionTrackingComponent(inner, TimeProvider.System);
        var pipeline = new ResiliencePipeline(component, Polly.Utils.DisposeBehavior.Allow, null);

        for (int i = 0; i < 10; i++)
        {
            _ = Task.Run(() => pipeline.Execute(() => { }));
        }

        while (tasks.Count != 10)
        {
            await Task.Delay(1);
        }

        var disposeTask = component.DisposeAsync().AsTask();

        while (tasks.Count > 1)
        {
            tasks.TryDequeue(out var ev).ShouldBeTrue();
            ev!.Set();
            ev.Dispose();
            disposeTask.Wait(1).ShouldBeFalse();
            inner.Disposed.ShouldBeFalse();
        }

        // last one
        tasks.TryDequeue(out var last).ShouldBeTrue();
        last!.Set();
        last.Dispose();
        await disposeTask;
        inner.Disposed.ShouldBeTrue();
    }

    private class Inner : PipelineComponent
    {
        public bool Disposed { get; private set; }

        public override ValueTask DisposeAsync()
        {
            Disposed = true;
            return default;
        }

        public Action OnExecute { get; set; } = () => { };

        internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
        {
            OnExecute();

            return await callback(context, state);
        }
    }
}
