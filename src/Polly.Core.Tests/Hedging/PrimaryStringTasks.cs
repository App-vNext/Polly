using Polly.Utils;

namespace Polly.Core.Tests.Hedging;

internal class PrimaryStringTasks
{
    public const string InstantTaskResult = "Instant";

    public const string FastTaskResult = "I am fast!";

    public const string SlowTaskResult = "I am so slow!";

    private readonly TimeProvider _timeProvider;

    public PrimaryStringTasks(TimeProvider timeProvider) => _timeProvider = timeProvider;

    public static Task<string> InstantTask()
    {
        return Task.FromResult(InstantTaskResult);
    }

    public async Task<string> FastTask(CancellationToken token)
    {
        await _timeProvider.Delay(TimeSpan.FromMilliseconds(10), token);
        return FastTaskResult;
    }

    public async Task<string> SlowTask(CancellationToken token)
    {
        await _timeProvider.Delay(TimeSpan.FromDays(1), token);
        return SlowTaskResult;
    }
}
