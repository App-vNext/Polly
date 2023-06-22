namespace Polly.Core.Tests.Hedging;

internal class PrimaryStringTasks
{
    public const string InstantTaskResult = "Instant";

    public const string FastTaskResult = "I am fast!";

    public const string SlowTaskResult = "I am so slow!";

    private readonly TimeProvider _timeProvider;

    public PrimaryStringTasks(TimeProvider timeProvider) => _timeProvider = timeProvider;

    public static ValueTask<string> InstantTask()
    {
        return new ValueTask<string>(InstantTaskResult);
    }

    public async ValueTask<string> FastTask(CancellationToken token)
    {
        await _timeProvider.Delay(TimeSpan.FromMilliseconds(10), token);
        return FastTaskResult;
    }

    public async ValueTask<string> SlowTask(CancellationToken token)
    {
        await _timeProvider.Delay(TimeSpan.FromDays(1), token);
        return SlowTaskResult;
    }
}
