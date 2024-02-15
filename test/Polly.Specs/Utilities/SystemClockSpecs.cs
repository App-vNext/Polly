namespace Polly.Specs.Utilities;

public class SystemClockSpecs
{
    [Fact]
    public void Sleep_ShouldNotThrow_WhenCancellationNotRequested() =>
        SystemClock.Sleep.Invoking(s =>
        {
            using var cts = new CancellationTokenSource();
            s(TimeSpan.FromMilliseconds(1), cts.Token);
        }).Should().NotThrow();

    [Fact]
    public void Sleep_ShouldThrow_WhenCancellationRequested() =>
        SystemClock.Sleep.Invoking(s =>
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            s(TimeSpan.FromMilliseconds(1), cts.Token);
        }).Should().Throw<OperationCanceledException>();

    [Fact]
    public async Task SleepAsync_ShouldNotThrow_WhenCancellationNotRequested() =>
        await SystemClock.SleepAsync.Invoking(async s =>
        {
            using var cts = new CancellationTokenSource();
            await s(TimeSpan.FromMilliseconds(1), cts.Token);
        }).Should().NotThrowAsync();

    [Fact]
    public async Task SleepAsync_ShouldThrow_WhenCancellationRequested() =>
        await SystemClock.SleepAsync.Invoking(async s =>
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await s(TimeSpan.FromMilliseconds(1), cts.Token);
        }).Should().ThrowAsync<OperationCanceledException>();
}
