using Polly;

namespace Chaos;

/// <summary>
/// Abstraction for controlling chaos injection.
/// </summary>
public interface IChaosManager
{
    ValueTask<bool> IsChaosEnabledAsync(ResilienceContext context);

    ValueTask<double> GetInjectionRateAsync(ResilienceContext context);
}