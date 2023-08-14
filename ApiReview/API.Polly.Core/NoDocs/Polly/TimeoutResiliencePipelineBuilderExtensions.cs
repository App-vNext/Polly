// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;
using Polly.Timeout;

namespace Polly;

public static class TimeoutResiliencePipelineBuilderExtensions
{
    public static TBuilder AddTimeout<TBuilder>(this TBuilder builder, TimeSpan timeout) where TBuilder : ResiliencePipelineBuilderBase;
    public static TBuilder AddTimeout<TBuilder>(this TBuilder builder, TimeoutStrategyOptions options) where TBuilder : ResiliencePipelineBuilderBase;
}
