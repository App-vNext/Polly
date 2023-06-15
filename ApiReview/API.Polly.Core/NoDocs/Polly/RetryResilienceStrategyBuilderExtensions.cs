// Assembly 'Polly.Core'

using System;
using Polly.Retry;

namespace Polly;

public static class RetryResilienceStrategyBuilderExtensions
{
    public static ResilienceStrategyBuilder<TResult> AddRetry<TResult>(this ResilienceStrategyBuilder<TResult> builder, Action<PredicateBuilder<TResult>> shouldRetry, RetryBackoffType backoffType, int retryCount, TimeSpan baseDelay);
    public static ResilienceStrategyBuilder AddRetry(this ResilienceStrategyBuilder builder, RetryStrategyOptions options);
    public static ResilienceStrategyBuilder<TResult> AddRetry<TResult>(this ResilienceStrategyBuilder<TResult> builder, RetryStrategyOptions<TResult> options);
}
