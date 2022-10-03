#nullable enable
using System;

namespace Polly.Retry.Options;

public class DelegateSleepDurationProvider : SleepDurationProviderBase
{
    private readonly Func<int, Exception, Context, TimeSpan> _sleepDurationProvider;

    public DelegateSleepDurationProvider(Func<int, Exception, Context, TimeSpan> sleepDurationProvider)
    {
        _sleepDurationProvider = sleepDurationProvider;
    }

    public override TimeSpan GetNext(int iteration, Exception exception, Context context)
        => _sleepDurationProvider.Invoke(iteration, exception, context);
}

public class DelegateSleepDurationProvider<TResult> : SleepDurationProviderBase<TResult>
{
    private readonly Func<int, DelegateResult<TResult>, Context, TimeSpan> _sleepDurationProvider;

    public DelegateSleepDurationProvider(Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider)
    {
        _sleepDurationProvider = sleepDurationProvider;
    }

    public override TimeSpan GetNext(int iteration, DelegateResult<TResult> outcome, Context context)
        => _sleepDurationProvider.Invoke(iteration, outcome, context);
}