namespace Polly;

/// <summary>
/// Resilience strategy is used to execute the user-provided callbacks.
/// </summary>
/// <typeparam name="T">The type of result this strategy supports.</typeparam>
/// <remarks>
/// Resilience strategy supports various types of callbacks of <typeparamref name="T"/> result type
/// and provides a unified way to execute them. This includes overloads for synchronous and asynchronous callbacks.
/// </remarks>
public partial class ResilienceStrategy<T>
{
    internal ResilienceStrategy(ResilienceStrategy strategy) => Strategy = strategy;

    internal ResilienceStrategy Strategy { get; }
}
