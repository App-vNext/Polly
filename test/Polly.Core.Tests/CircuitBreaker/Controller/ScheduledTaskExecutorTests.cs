using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker.Controller;

public class ScheduledTaskExecutorTests
{
    [Fact]
    public async Task ScheduleTask_Success_EnsureExecuted()
    {
        using var scheduler = new ScheduledTaskExecutor();
        var executed = false;
        scheduler.ScheduleTask(
            () =>
            {
                executed = true;
                return Task.CompletedTask;
            },
            ResilienceContextPool.Shared.Get(),
            out var task);

        await task;

        executed.Should().BeTrue();
    }

    [Fact]
    public async Task ScheduleTask_OperationCanceledException_EnsureExecuted()
    {
        using var scheduler = new ScheduledTaskExecutor();
        scheduler.ScheduleTask(
            () => throw new OperationCanceledException(),
            ResilienceContextPool.Shared.Get(),
            out var task);

        await task.Invoking(async t => await task).Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ScheduleTask_Exception_EnsureExecuted()
    {
        using var scheduler = new ScheduledTaskExecutor();
        scheduler.ScheduleTask(
            () => throw new InvalidOperationException(),
            ResilienceContextPool.Shared.Get(),
            out var task);

        await task.Invoking(async t => await task).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ScheduleTask_Multiple_EnsureExecutionSerialized()
    {
        using var executing = new ManualResetEvent(false);
        using var verified = new ManualResetEvent(false);

        using var scheduler = new ScheduledTaskExecutor();
        scheduler.ScheduleTask(
            () =>
            {
                executing.Set();
                verified.WaitOne();
                return Task.CompletedTask;
            },
            ResilienceContextPool.Shared.Get(),
            out var task);

        executing.WaitOne();

        scheduler.ScheduleTask(() => Task.CompletedTask, ResilienceContextPool.Shared.Get(), out var otherTask);
        otherTask.Wait(50).Should().BeFalse();

        verified.Set();

        await task;
        await otherTask;
    }

    [Fact]
    public async Task Dispose_ScheduledTaskCancelled()
    {
        using var executing = new ManualResetEvent(false);
        using var verified = new ManualResetEvent(false);

        var scheduler = new ScheduledTaskExecutor();
        scheduler.ScheduleTask(
            () =>
            {
                executing.Set();
                verified.WaitOne();
                return Task.CompletedTask;
            },
            ResilienceContextPool.Shared.Get(),
            out var task);

        executing.WaitOne();
        scheduler.ScheduleTask(() => Task.CompletedTask, ResilienceContextPool.Shared.Get(), out var otherTask);
        scheduler.Dispose();
        verified.Set();
        await task;

        await otherTask.Invoking(t => otherTask).Should().ThrowAsync<OperationCanceledException>();

        scheduler
            .Invoking(s => s.ScheduleTask(() => Task.CompletedTask, ResilienceContextPool.Shared.Get(), out _))
            .Should()
            .Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_WhenScheduledTaskExecuting()
    {
        using var disposed = new ManualResetEvent(false);
        using var ready = new ManualResetEvent(false);

        var scheduler = new ScheduledTaskExecutor();
        scheduler.ScheduleTask(
            () =>
            {
                ready.Set();
                disposed.WaitOne();
                return Task.CompletedTask;
            },
            ResilienceContextPool.Shared.Get(),
            out var task);

        ready.WaitOne(TimeSpan.FromSeconds(10)).Should().BeTrue();
        scheduler.Dispose();
        disposed.Set();

        scheduler.ProcessingTask.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();
    }

    [Fact]
    public async Task Dispose_EnsureNoBackgroundProcessing()
    {
        var scheduler = new ScheduledTaskExecutor();
        scheduler.ScheduleTask(() => Task.CompletedTask, ResilienceContextPool.Shared.Get(), out var otherTask);
        await otherTask;
        scheduler.Dispose();
#pragma warning disable S3966 // Objects should not be disposed more than once
        scheduler.Dispose();
#pragma warning restore S3966 // Objects should not be disposed more than once

        await scheduler.ProcessingTask;

        scheduler.ProcessingTask.IsCompleted.Should().BeTrue();
    }
}
