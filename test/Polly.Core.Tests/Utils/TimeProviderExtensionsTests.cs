using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class TimeProviderExtensionsTests
{
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [Theory]
    public async Task DelayAsync_System_Ok(bool synchronous, bool hasCancellation)
    {
        using var tcs = new CancellationTokenSource();
        var token = hasCancellation ? tcs.Token : default;
        var delay = TimeSpan.FromMilliseconds(10);
        var timeProvider = TimeProvider.System;
        var context = ResilienceContext.Get();
        context.Initialize<VoidResult>(isSynchronous: synchronous);
        context.CancellationToken = token;

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            var task = timeProvider.DelayAsync(delay, context);
            task.IsCompleted.Should().Be(synchronous);
            await task;
        });
    }

    [Fact]
    public async Task DelayAsync_SystemSynchronous_Ok()
    {
        var delay = TimeSpan.FromMilliseconds(5);
        var context = ResilienceContext.Get();
        context.Initialize<VoidResult>(isSynchronous: true);

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            var watch = Stopwatch.StartNew();
            await TimeProvider.System.DelayAsync(delay, context);
            var elapsed = watch.Elapsed;
            elapsed.Should().BeGreaterThanOrEqualTo(delay);
        });
    }

    [Fact]
    public async Task DelayAsync_SystemSynchronousWhenCancelled_Ok()
    {
        using var cts = new CancellationTokenSource(5);
        var delay = TimeSpan.FromMilliseconds(10);
        var context = ResilienceContext.Get();
        context.Initialize<VoidResult>(isSynchronous: true);
        context.CancellationToken = cts.Token;

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            await TimeProvider.System
                .Invoking(p => p.DelayAsync(delay, context))
                .Should()
                .ThrowAsync<OperationCanceledException>();
        });
    }

    [InlineData(false)]
    [InlineData(true)]
    [Theory]
    public async Task DelayAsync_CancellationRequestedBefore_Throws(bool synchronous)
    {
        using var tcs = new CancellationTokenSource();
        tcs.Cancel();
        var token = tcs.Token;
        var delay = TimeSpan.FromMilliseconds(10);
        var timeProvider = TimeProvider.System;
        var context = ResilienceContext.Get();
        context.Initialize<VoidResult>(isSynchronous: synchronous);
        context.CancellationToken = token;

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
            var token = tcs.Token;
            var timeProvider = TimeProvider.System;
            var context = ResilienceContext.Get();
            context.Initialize<VoidResult>(isSynchronous: synchronous);
            context.CancellationToken = token;

            tcs.CancelAfter(TimeSpan.FromMilliseconds(5));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => timeProvider.DelayAsync(delay, context));
        });
    }
}
