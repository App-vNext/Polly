using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class TimeProviderExtensionsTests
{
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [Theory]
    public async Task DelayAsync_System_Ok(bool synchronous, bool mocked, bool hasCancellation)
    {
        using var tcs = new CancellationTokenSource();
        var token = hasCancellation ? tcs.Token : default;
        var delay = TimeSpan.FromMilliseconds(10);
        var mock = new MockTimeProvider();
        var timeProvider = mocked ? mock.Object : TimeProvider.System;
        var context = ResilienceContext.Get();
        context.Initialize<VoidResult>(isSynchronous: synchronous);
        context.CancellationToken = token;
        mock.SetupCreateTimer(delay);

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            var task = timeProvider.DelayAsync(delay, context);
            task.IsCompleted.Should().Be(synchronous || mocked);
            await task;
        });

        if (mocked)
        {
            mock.VerifyAll();
        }
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

    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [Theory]
    public async Task DelayAsync_CancellationRequestedBefore_Throws(bool synchronous, bool mocked)
    {
        using var tcs = new CancellationTokenSource();
        tcs.Cancel();
        var token = tcs.Token;
        var delay = TimeSpan.FromMilliseconds(10);
        var mock = new MockTimeProvider();
        var timeProvider = mocked ? mock.Object : TimeProvider.System;
        var context = ResilienceContext.Get();
        context.Initialize<VoidResult>(isSynchronous: synchronous);
        context.CancellationToken = token;
        mock.SetupCreateTimer(delay);

        await Assert.ThrowsAsync<OperationCanceledException>(() => timeProvider.DelayAsync(delay, context));
    }

    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [Theory]
    public async Task DelayAsync_CancellationAfter_Throws(bool synchronous, bool mocked)
    {
        var delay = TimeSpan.FromMilliseconds(20);

        await TestUtilities.AssertWithTimeoutAsync(async () =>
        {
            var mock = new MockTimeProvider();
            using var tcs = new CancellationTokenSource();
            var token = tcs.Token;
            var timeProvider = mocked ? mock.Object : TimeProvider.System;
            var context = ResilienceContext.Get();
            context.Initialize<VoidResult>(isSynchronous: synchronous);
            context.CancellationToken = token;
            mock.SetupCreateTimerException(delay, new OperationCanceledException());

            tcs.CancelAfter(TimeSpan.FromMilliseconds(5));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => timeProvider.DelayAsync(delay, context));
        });
    }
}
