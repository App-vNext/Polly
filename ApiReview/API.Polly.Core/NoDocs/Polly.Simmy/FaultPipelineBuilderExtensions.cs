// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;
using Polly.Simmy.Fault;

namespace Polly.Simmy;

public static class FaultPipelineBuilderExtensions
{
    public static TBuilder AddChaosFault<TBuilder>(this TBuilder builder, double injectionRate, Exception fault) where TBuilder : ResiliencePipelineBuilderBase;
    public static TBuilder AddChaosFault<TBuilder>(this TBuilder builder, double injectionRate, Func<Exception?> faultGenerator) where TBuilder : ResiliencePipelineBuilderBase;
    public static TBuilder AddChaosFault<TBuilder>(this TBuilder builder, FaultStrategyOptions options) where TBuilder : ResiliencePipelineBuilderBase;
}
