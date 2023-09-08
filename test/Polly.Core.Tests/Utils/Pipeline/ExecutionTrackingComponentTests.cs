using System;
using System.Threading.Tasks;
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
        var execution = Task.Run(() => new ResiliencePipeline(component, Polly.Utils.DisposeBehavior.Allow).Execute(() => { }));
        executing.WaitOne();

        var disposeTask = component.DisposeAsync().AsTask();
        _timeProvider.Advance(ExecutionTrackingComponent.SleepDelay);
        inner.Disposed.Should().BeFalse();
        assert.Set();

        _timeProvider.Advance(ExecutionTrackingComponent.SleepDelay);
        await execution;

        _timeProvider.Advance(ExecutionTrackingComponent.SleepDelay);
        await disposeTask;

        inner.Disposed.Should().BeTrue();
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
        var execution = Task.Run(() => new ResiliencePipeline(component, Polly.Utils.DisposeBehavior.Allow).Execute(() => { }));
        executing.WaitOne();

        var disposeTask = component.DisposeAsync().AsTask();
        inner.Disposed.Should().BeFalse();
        _timeProvider.Advance(ExecutionTrackingComponent.Timeout - TimeSpan.FromSeconds(1));
        inner.Disposed.Should().BeFalse();
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        await disposeTask;
        inner.Disposed.Should().BeTrue();

        assert.Set();
        await execution;
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
