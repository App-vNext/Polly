namespace Polly.Specs.Utilities;

[Collection(Constants.SystemClockDependentTestCollection)]
public class SystemClockSpecs
{
    private readonly Action<TimeSpan, CancellationToken> _sleep;
    private readonly Func<TimeSpan, CancellationToken, Task> _sleepAsync;

    public SystemClockSpecs()
    {
        SystemClock.Reset();
        _sleep = SystemClock.Sleep;
        _sleepAsync = SystemClock.SleepAsync;
    }

    [Fact]
    public void Sleep_ShouldNotThrow_WhenCancellationNotRequested() =>
        _sleep.Invoking(s =>
        {
            using var cts = new CancellationTokenSource();
            s(TimeSpan.FromMilliseconds(1), cts.Token);
        }).Should().NotThrow();

    [Fact]
    public void Sleep_ShouldThrow_WhenCancellationRequested() =>
        _sleep.Invoking(s =>
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            s(TimeSpan.FromMilliseconds(1), cts.Token);
        }).Should().Throw<OperationCanceledException>();

    [Fact]
    public async Task SleepAsync_ShouldNotThrow_WhenCancellationNotRequested() =>
        await _sleepAsync.Invoking(async s =>
        {
            using var cts = new CancellationTokenSource();
            await s(TimeSpan.FromMilliseconds(1), cts.Token);
        }).Should().NotThrowAsync();

    [Fact]
    public async Task SleepAsync_ShouldThrow_WhenCancellationRequested() =>
        await _sleepAsync.Invoking(async s =>
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await s(TimeSpan.FromMilliseconds(1), cts.Token);
        }).Should().ThrowAsync<OperationCanceledException>();
}
