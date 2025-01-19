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
    public void Sleep_Should_NotThrow_WhenCancellationNotRequested() =>
        Should.NotThrow(() =>
        {
            using var cts = new CancellationTokenSource();
            _sleep(TimeSpan.FromMilliseconds(1), cts.Token);
        });

    [Fact]
    public void Sleep_Should_Throw_WhenCancellationRequested() =>
        Should.Throw<OperationCanceledException>(() =>
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            _sleep(TimeSpan.FromMilliseconds(1), cts.Token);
        });

    [Fact]
    public async Task SleepAsync_Should_NotThrow_WhenCancellationNotRequested() =>
        await Should.NotThrowAsync(async () =>
        {
            using var cts = new CancellationTokenSource();
            await _sleepAsync(TimeSpan.FromMilliseconds(1), cts.Token);
        });

    [Fact]
    public async Task SleepAsync_Should_Throw_WhenCancellationRequested() =>
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await _sleepAsync(TimeSpan.FromMilliseconds(1), cts.Token);
        });
}
