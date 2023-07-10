using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Hedging;
using Polly.Retry;
using Polly.Timeout;
using Polly.Utils;

namespace Polly.Testing;

/// <summary>
/// The test-related extensions for <see cref="ResilienceStrategy"/> and <see cref="ResilienceStrategy{TResult}"/>.
/// </summary>
public static class ResilienceStrategyExtensions
{
    /// <summary>
    /// Gets the inner strategies the <paramref name="strategy"/> is composed of.
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>A list of inner strategies.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<ResilienceStrategyDescriptor> GetInnerStrategies<TResult>(this ResilienceStrategy<TResult> strategy)
    {
        Guard.NotNull(strategy);

        return strategy.Strategy.GetInnerStrategies();
    }

    /// <summary>
    /// Gets the inner strategies the <paramref name="strategy"/> is composed of.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>A list of inner strategies.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<ResilienceStrategyDescriptor> GetInnerStrategies(this ResilienceStrategy strategy)
    {
        Guard.NotNull(strategy);

        var strategies = new List<ResilienceStrategy>();
        strategy.ExpandStrategies(strategies);

        return strategies.Select(s => new ResilienceStrategyDescriptor(s.Options, GetType(s), s.GetType())).ToList();
    }

    private static void ExpandStrategies(this ResilienceStrategy strategy, List<ResilienceStrategy> strategies)
    {
        if (strategy is ResilienceStrategyPipeline pipeline)
        {
            foreach (var inner in pipeline.Strategies)
            {
                inner.ExpandStrategies(strategies);
            }
        }
        else if (strategy is ReloadableResilienceStrategy reloadable)
        {
            strategies.Add(reloadable);
            ExpandStrategies(reloadable.Strategy, strategies);
        }
        else
        {
            strategies.Add(strategy);
        }
    }

    private static ResilienceStrategyType GetType(ResilienceStrategy strategy) => strategy switch
    {
        TimeoutResilienceStrategy => ResilienceStrategyType.Timeout,
        ReloadableResilienceStrategy => ResilienceStrategyType.Reload,
        _ when strategy.GetType().FullName == "Polly.RateLimiting.RateLimiterResilienceStrategy" => ResilienceStrategyType.RateLimiter,
        _ when strategy.GetType().FullName == "Polly.Extensions.Telemetry.TelemetryResilienceStrategy" => ResilienceStrategyType.Telemetry,
        _ when strategy.GetType().IsGenericType && strategy.GetType().GetGenericTypeDefinition() == typeof(RetryResilienceStrategy<>) => ResilienceStrategyType.Retry,
        _ when strategy.GetType().IsGenericType && strategy.GetType().GetGenericTypeDefinition() == typeof(CircuitBreakerResilienceStrategy<>) => ResilienceStrategyType.CircuitBreaker,
        _ when strategy.GetType().IsGenericType && strategy.GetType().GetGenericTypeDefinition() == typeof(HedgingResilienceStrategy<>) => ResilienceStrategyType.Hedging,
        _ when strategy.GetType().IsGenericType && strategy.GetType().GetGenericTypeDefinition() == typeof(FallbackResilienceStrategy<>) => ResilienceStrategyType.Fallback,

        _ => ResilienceStrategyType.Custom
    };
}
