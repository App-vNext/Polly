// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Retry;

namespace Polly;

public static class RetryCompositeStrategyBuilderExtensions
{
    public static CompositeStrategyBuilder AddRetry(this CompositeStrategyBuilder builder, RetryStrategyOptions options);
    public static CompositeStrategyBuilder<TResult> AddRetry<TResult>(this CompositeStrategyBuilder<TResult> builder, RetryStrategyOptions<TResult> options);
}
