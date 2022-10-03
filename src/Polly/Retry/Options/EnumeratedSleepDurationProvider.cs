#nullable enable
using System;
using System.Collections.Generic;

namespace Polly.Retry.Options;

internal class SleepDurationProviderEnumerableAdapter : SleepDurationProviderBase
{
    private readonly IEnumerator<TimeSpan> _enumerator;

    public SleepDurationProviderEnumerableAdapter(IEnumerable<TimeSpan> sleepDurations)
    {
        if (sleepDurations is null) throw new ArgumentNullException(nameof(sleepDurations));
        _enumerator = sleepDurations.GetEnumerator();
    }
    
    public override TimeSpan GetNext(int iteration, Exception exception, Context context)
    {
        if (!_enumerator.MoveNext()) 
            throw new InvalidOperationException("It seems that the enumerator has only a fixed amount of elements.");
        return _enumerator.Current;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        _enumerator.Dispose();
    }
}
