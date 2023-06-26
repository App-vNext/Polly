namespace Polly.Core.Tests.Hedging;

internal class HedgingTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public HedgingTimeProvider() => _utcNow = DateTimeOffset.UtcNow;

    public TimeSpan AutoAdvance { get; set; }

    public void Advance(TimeSpan diff)
    {
        _utcNow = _utcNow.Add(diff);

        foreach (var entry in DelayEntries.Where(e => e.TimeStamp <= _utcNow))
        {
            entry.Complete();
        }
    }

    public Func<int> TimeStampProvider { get; set; } = () => 0;

    public List<DelayEntry> DelayEntries { get; } = new List<DelayEntry>();

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public override long GetTimestamp() => TimeStampProvider();

    public override Task Delay(TimeSpan delayValue, CancellationToken cancellationToken = default)
    {
        var entry = new DelayEntry(delayValue, new TaskCompletionSource<bool>(), _utcNow.Add(delayValue));
        cancellationToken.Register(() => entry.Source.TrySetCanceled(cancellationToken));
        DelayEntries.Add(entry);

        Advance(AutoAdvance);

        return entry.Source.Task;
    }

    public record DelayEntry(TimeSpan Delay, TaskCompletionSource<bool> Source, DateTimeOffset TimeStamp)
    {
        public void Complete() => Source.TrySetResult(true);
    }
}
