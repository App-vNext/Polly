// Assembly 'Polly.Core'

using System;
using Polly.Timeout;

namespace Polly;

public static class TimeoutResilienceStrategyBuilderExtensions
{
    public static TBuilder AddTimeout<TBuilder>(this TBuilder builder, TimeSpan timeout) where TBuilder : ResilienceStrategyBuilderBase;
    public static TBuilder AddTimeout<TBuilder>(this TBuilder builder, TimeoutStrategyOptions options) where TBuilder : ResilienceStrategyBuilderBase;
}
