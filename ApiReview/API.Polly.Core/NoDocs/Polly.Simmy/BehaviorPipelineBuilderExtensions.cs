// Assembly 'Polly.Core'

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Polly.Simmy.Behavior;

namespace Polly.Simmy;

public static class BehaviorPipelineBuilderExtensions
{
    public static TBuilder AddChaosBehavior<TBuilder>(this TBuilder builder, bool enabled, double injectionRate, Func<ValueTask> behavior) where TBuilder : ResiliencePipelineBuilderBase;
    public static TBuilder AddChaosBehavior<TBuilder>(this TBuilder builder, BehaviorStrategyOptions options) where TBuilder : ResiliencePipelineBuilderBase;
}
