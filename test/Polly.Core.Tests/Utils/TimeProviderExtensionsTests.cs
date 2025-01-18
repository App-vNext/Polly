using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class TimeProviderExtensionsTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task DelayAsync_System_Ok(bool synchronous, bool hasCancellation)
    {
        using var tcs = new CancellationTokenSource();
        var token = hasCancellation ? tcs.Token : default;
        var delay = TimeSpan.FromMilliseconds(10);
        var timeProvider = TimeProvider.System;
        var context = ResilienceContextPool.Shared.Get(token);
        context.Initialize<VoidResult>(isSynchronous: synchronous);

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            var task = timeProvider.DelayAsync(delay, context);
            task.IsCompleted.ShouldBe(synchronous);
            await task;
        });
    }

    [Fact]
    public async Task DelayAsync_SystemSynchronous_Ok()
    {
        var delay = TimeSpan.FromMilliseconds(5);
        var context = ResilienceContextPool.Shared.Get();
        context.Initialize<VoidResult>(isSynchronous: true);

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            var watch = Stopwatch.StartNew();
            await TimeProvider.System.DelayAsync(delay, context);
            var elapsed = watch.Elapsed;
            elapsed.ShouldBeGreaterThanOrEqualTo(delay);
        });
    }

    [Fact]
    public async Task DelayAsync_SystemSynchronousWhenCancelled_Ok()
    {
        using var cts = new CancellationTokenSource(5);
        var delay = TimeSpan.FromMilliseconds(10);
        var context = ResilienceContextPool.Shared.Get(cts.Token);
        context.Initialize<VoidResult>(isSynchronous: true);

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            await Should.ThrowAsync<OperationCanceledException>(
                () => TimeProvider.System.DelayAsync(delay, context));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DelayAsync_CancellationRequestedBefore_Throws(bool synchronous)
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var delay = TimeSpan.FromMilliseconds(10);
        var timeProvider = TimeProvider.System;
        var context = ResilienceContextPool.Shared.Get(cts.Token);
        context.Initialize<VoidResult>(isSynchronous: synchronous);

        await Assert.ThrowsAsync<OperationCanceledException>(() => timeProvider.DelayAsync(delay, context));
    }

    [InlineData(false)]
    [InlineData(true)]
    [Theory]
    public async Task DelayAsync_CancellationAfter_Throws(bool synchronous)
    {
        var delay = TimeSpan.FromMilliseconds(20);

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            using var tcs = new CancellationTokenSource();
            var timeProvider = TimeProvider.System;
            var context = ResilienceContextPool.Shared.Get(tcs.Token);
            context.Initialize<VoidResult>(isSynchronous: synchronous);

            tcs.CancelAfter(TimeSpan.FromMilliseconds(5));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => timeProvider.DelayAsync(delay, context));
        });
    }

    [Fact]
    public async Task DelayAsync_MaxTimeSpan_DoesNotThrow()
    {
        var delay = TimeSpan.MaxValue;

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            using var cts = new CancellationTokenSource();
            var timeProvider = TimeProvider.System;
            var context = ResilienceContextPool.Shared.Get(cts.Token);
            context.Initialize<VoidResult>(isSynchronous: false);

            var delayTask = timeProvider.DelayAsync(delay, context);
            delayTask.Wait(TimeSpan.FromMilliseconds(10)).ShouldBeFalse();

            cts.Cancel();

            await Should.ThrowAsync<OperationCanceledException>(() => delayTask);
        });
    }
}
