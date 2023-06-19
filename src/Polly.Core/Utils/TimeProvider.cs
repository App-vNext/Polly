namespace Polly.Utils;

#pragma warning disable S3872 // Parameter names should not duplicate the names of their methods

/// <summary>
/// TEMPORARY ONLY, to be replaced with System.TimeProvider - https://github.com/dotnet/runtime/issues/36617 later.
/// </summary>
/// <remarks>We trimmed some of the API that's not relevant for us too.</remarks>
internal abstract class TimeProvider
{
    private readonly double _tickFrequency;

    public static TimeProvider System { get; } = new SystemTimeProvider();

    protected TimeProvider(long timestampFrequency)
    {
        TimestampFrequency = timestampFrequency;
        _tickFrequency = (double)TimeSpan.TicksPerSecond / TimestampFrequency;
    }

    public abstract DateTimeOffset UtcNow { get; }

    public long TimestampFrequency { get; }

    public virtual long GetTimestamp() => Environment.TickCount;

    public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) => new((long)((endingTimestamp - startingTimestamp) * _tickFrequency));

    public TimeSpan GetElapsedTime(long startingTimestamp) => GetElapsedTime(startingTimestamp, GetTimestamp());

    public abstract Task Delay(TimeSpan delay, CancellationToken cancellationToken = default);

    public abstract void CancelAfter(CancellationTokenSource source, TimeSpan delay);

    private sealed class SystemTimeProvider : TimeProvider
    {
        public SystemTimeProvider()
            : base(Stopwatch.Frequency)
        {
        }

        public override long GetTimestamp() => Stopwatch.GetTimestamp();

        public override Task Delay(TimeSpan delay, CancellationToken cancellationToken = default) => Task.Delay(delay, cancellationToken);

        public override void CancelAfter(CancellationTokenSource source, TimeSpan delay) => source.CancelAfter(delay);

        public override DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
