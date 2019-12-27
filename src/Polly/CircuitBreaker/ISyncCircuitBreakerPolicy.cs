namespace Polly.CircuitBreaker
{
    /// <summary>
    /// A circuit-breaker policy that can be applied to synchronous executions.
    /// </summary>
    public interface ISyncCircuitBreakerPolicy : ISyncPolicy, ICircuitBreakerPolicy
    {
    }

    /// <summary>
    /// A circuit-breaker policy that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface ISyncCircuitBreakerPolicy<TResult> : ISyncPolicy<TResult>, ICircuitBreakerPolicy<TResult>
    {
    }
}
