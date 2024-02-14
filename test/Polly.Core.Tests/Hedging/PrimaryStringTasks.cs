namespace Polly.Core.Tests.Hedging;

internal class PrimaryStringTasks
{
    public const string InstantTaskResult = "Instant";

    public const string FastTaskResult = "I am fast!";

    public const string SlowTaskResult = "I am so slow!";

    private readonly TimeProvider _timeProvider;

    public PrimaryStringTasks(TimeProvider timeProvider) => _timeProvider = timeProvider;

    public static ValueTask<string> InstantTask() =>
        new(InstantTaskResult);

    public async ValueTask<string> FastTask(CancellationToken token)
    {
#if NET8_0_OR_GREATER
        await Task.Delay(TimeSpan.FromMilliseconds(10), _timeProvider, token);
#else
        await _timeProvider.Delay(TimeSpan.FromMilliseconds(10), token);
#endif
        return FastTaskResult;
    }

    public async ValueTask<string> SlowTask(CancellationToken token)
    {
#if NET8_0_OR_GREATER
        await Task.Delay(TimeSpan.FromDays(1), _timeProvider, token);
#else
        await _timeProvider.Delay(TimeSpan.FromDays(1), token);
#endif

        return SlowTaskResult;
    }
}
