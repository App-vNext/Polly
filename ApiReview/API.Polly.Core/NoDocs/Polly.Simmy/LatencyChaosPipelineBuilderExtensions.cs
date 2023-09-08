// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Latency;

namespace Polly.Simmy;

public static class LatencyChaosPipelineBuilderExtensions
{
    public static TBuilder AddChaosLatency<TBuilder>(this TBuilder builder, bool enabled, double injectionRate, TimeSpan latency) where TBuilder : ResiliencePipelineBuilderBase;
    public static TBuilder AddChaosLatency<TBuilder>(this TBuilder builder, LatencyStrategyOptions options) where TBuilder : ResiliencePipelineBuilderBase;
}
