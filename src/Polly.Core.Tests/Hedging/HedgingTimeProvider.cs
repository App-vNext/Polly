using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Core.Tests.Hedging;

internal class HedgingTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public HedgingTimeProvider() => _utcNow = DateTimeOffset.UtcNow;

    public TimeSpan AutoAdvance { get; set; }

    public override long TimestampFrequency => Stopwatch.Frequency;

    public void Advance(TimeSpan diff)
    {
        _utcNow = _utcNow.Add(diff);

        foreach (var entry in TimerEntries.Where(e => e.TimeStamp <= _utcNow))
        {
            entry.Complete();
        }
    }

    public List<TimerEntry> TimerEntries { get; } = new List<TimerEntry>();

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var entry = new TimerEntry(dueTime, new TaskCompletionSource<bool>(), _utcNow.Add(dueTime), () => callback(state));
        TimerEntries.Add(entry);

        Advance(AutoAdvance);

        return entry;
    }

    public override long GetTimestamp() => throw new NotSupportedException();

    public record TimerEntry(TimeSpan Delay, TaskCompletionSource<bool> Source, DateTimeOffset TimeStamp, Action Callback) : ITimer
    {
        public bool Change(TimeSpan dueTime, TimeSpan period) => throw new NotSupportedException();

        public void Complete()
        {
            Callback();
            Source.TrySetResult(true);
        }

        public void Dispose() => Source.TrySetResult(true);

        public ValueTask DisposeAsync()
        {
            Source.TrySetResult(true);
            return default;
        }
    }
}
