#nullable enable
namespace Polly.Retry.Options;

public class RetryPolicyOptions : RetryPolicyOptionsBase
{
    public RetryInvocationHandlerBase? RetryInvocationHandler { get; set; }
    public SleepDurationProviderBase? SleepDurationProvider { get; set; }
}

public class RetryPolicyOptions<TResult> : RetryPolicyOptionsBase
{
    public RetryInvocationHandlerBase<TResult>? RetryInvocationHandler { get; set; }
    public SleepDurationProviderBase<TResult>? SleepDurationProvider { get; set; }
}
