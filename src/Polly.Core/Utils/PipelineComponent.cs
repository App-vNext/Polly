using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly.Telemetry;

namespace Polly.Utils;

internal abstract partial class PipelineComponent
{
    public static PipelineComponent Null { get; } = null!;

    internal ResilienceStrategyOptions? Options { get; set; }

    internal static PipelineComponent FromStrategy(ResilienceStrategy strategy) => new Bridge(strategy);

    internal static PipelineComponent FromStrategy<T>(ResilienceStrategy<T> strategy) => new Bridge<T>(strategy);

    internal static PipelineComponent CreateComposite(
        IReadOnlyList<PipelineComponent> components,
        ResilienceStrategyTelemetry telemetry,
        TimeProvider timeProvider)
    {
        Guard.NotNull(components);

        if (components.Count == 0)
        {
            throw new InvalidOperationException("The composite resilience pipeline must contain at least one resilience strategy.");
        }

        if (components.Distinct().Count() != components.Count)
        {
            throw new InvalidOperationException("The composite resilience pipeline must contain unique resilience strategies.");
        }

        if (components.Count == 1)
        {
            return new CompositePipelineComponent(components[0], components, telemetry, timeProvider);
        }

        // convert all strategies to delegating ones (except the last one as it's not required)
        var delegatingStrategies = components
            .Take(components.Count - 1)
            .Select(strategy => new DelegatingPipelineComponent(strategy))
            .ToList();

#if NET6_0_OR_GREATER
        // link the last one
        delegatingStrategies[^1].Next = components[^1];
#else
        delegatingStrategies[delegatingStrategies.Count - 1].Next = components[components.Count - 1];
#endif

        // link the remaining ones
        for (var i = 0; i < delegatingStrategies.Count - 1; i++)
        {
            delegatingStrategies[i].Next = delegatingStrategies[i + 1];
        }

        return new CompositePipelineComponent(delegatingStrategies[0], components, telemetry, timeProvider);
    }

    internal static PipelineComponent CreateReloadable(IEnumerable<ResilienceStrategy> strategy) => null!;

    internal abstract ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state);
}
