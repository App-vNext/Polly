namespace Polly.Fallback
{
    /// <summary>
    /// A fallback policy that can be applied to synchronous delegate executions.
    /// </summary>
    public interface ISyncFallbackPolicy : ISyncPolicy, IFallbackPolicy
    {
    }

    /// <summary>
    /// A fallback policy that can be applied to synchronous delegate executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface ISyncFallbackPolicy<TResult> : ISyncPolicy<TResult>, IFallbackPolicy<TResult>
    {
    }
}
