#nullable enable
using System;

namespace Polly.Retry.Options;

internal class SleepDurationProviderAdapter<TResult> : SleepDurationProviderBase<TResult>
{
    private readonly SleepDurationProviderBase _provider;

    public SleepDurationProviderAdapter(SleepDurationProviderBase provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }
    
    public override TimeSpan GetNext(int iteration, DelegateResult<TResult> result, Context context) 
        => _provider.GetNext(iteration, result.Exception, context);
}