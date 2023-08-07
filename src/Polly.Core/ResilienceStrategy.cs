namespace Polly;

/// <summary>
/// Resilience strategy is used to execute the user-provided callbacks.
/// </summary>
/// <remarks>
/// Resilience strategy supports various types of callbacks and provides a unified way to execute them.
/// This includes overloads for synchronous and asynchronous callbacks, generic and non-generic callbacks.
/// </remarks>
public partial class ResilienceStrategy
{
    internal ResilienceStrategy(ResilienceStrategy<object> strategy) => Strategy = strategy;

    internal ResilienceStrategy<object> Strategy { get; }
}
