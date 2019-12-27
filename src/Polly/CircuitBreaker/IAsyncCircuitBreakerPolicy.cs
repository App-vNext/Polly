namespace Polly.CircuitBreaker
{
    /// <summary>
    /// A circuit-breaker policy that can be applied to asynchronous executions.
    /// </summary>
    public interface IAsyncCircuitBreakerPolicy : IAsyncPolicy, ICircuitBreakerPolicy
    {
    }

    /// <summary>
    /// A circuit-breaker policy that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface IAsyncCircuitBreakerPolicy<TResult> : IAsyncPolicy<TResult>, ICircuitBreakerPolicy<TResult>
    {
    }
}
