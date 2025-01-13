using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker.Controller;

public class ScheduledTaskExecutorTests
{
    private static CancellationToken CancellationToken => CancellationToken.None;

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
            ResilienceContextPool.Shared.Get(CancellationToken),
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
            ResilienceContextPool.Shared.Get(CancellationToken),
            out var task);

        await task.Invoking(async t => await task).Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ScheduleTask_Exception_EnsureExecuted()
    {
        using var scheduler = new ScheduledTaskExecutor();
        scheduler.ScheduleTask(
            () => throw new InvalidOperationException(),
            ResilienceContextPool.Shared.Get(CancellationToken),
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
            ResilienceContextPool.Shared.Get(CancellationToken),
            out var task);

        executing.WaitOne();

        scheduler.ScheduleTask(() => Task.CompletedTask, ResilienceContextPool.Shared.Get(CancellationToken), out var otherTask);

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        otherTask.Wait(50, CancellationToken).Should().BeFalse();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

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
            ResilienceContextPool.Shared.Get(CancellationToken),
            out var task);

        executing.WaitOne();
        scheduler.ScheduleTask(() => Task.CompletedTask, ResilienceContextPool.Shared.Get(CancellationToken), out var otherTask);
        scheduler.Dispose();
        verified.Set();
        await task;

        await otherTask.Invoking(t => otherTask).Should().ThrowAsync<OperationCanceledException>();

        scheduler
            .Invoking(s => s.ScheduleTask(() => Task.CompletedTask, ResilienceContextPool.Shared.Get(CancellationToken), out _))
            .Should()
            .Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_WhenScheduledTaskExecuting()
    {
        var timeout = TimeSpan.FromSeconds(10);

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
            ResilienceContextPool.Shared.Get(CancellationToken),
            out var task);

        ready.WaitOne(timeout).Should().BeTrue();
        scheduler.Dispose();
        disposed.Set();

#pragma warning disable xUnit1031
#if NET
        scheduler.ProcessingTask.Wait(timeout, CancellationToken).Should().BeTrue();
#else
        scheduler.ProcessingTask.Wait(timeout).Should().BeTrue();
#endif
#pragma warning restore xUnit1031
    }

    [Fact]
    public async Task Dispose_EnsureNoBackgroundProcessing()
    {
        var scheduler = new ScheduledTaskExecutor();
        scheduler.ScheduleTask(() => Task.CompletedTask, ResilienceContextPool.Shared.Get(CancellationToken), out var otherTask);
        await otherTask;
        scheduler.Dispose();
#pragma warning disable S3966 // Objects should not be disposed more than once
        scheduler.Dispose();
#pragma warning restore S3966 // Objects should not be disposed more than once

        await scheduler.ProcessingTask;

        scheduler.ProcessingTask.IsCompleted.Should().BeTrue();
    }
}
