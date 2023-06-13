namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.ShouldHandle"/> predicate.
/// </summary>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly record struct CircuitBreakerPredicateArguments();
