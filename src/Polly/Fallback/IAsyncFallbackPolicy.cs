namespace Polly.Fallback
{
    /// <summary>
    /// A fallback policy that can be applied to asynchronous executions.
    /// </summary>
    public interface IAsyncFallbackPolicy : IAsyncPolicy, IFallbackPolicy
    {
    }

    /// <summary>
    /// A fallback policy that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface IAsyncFallbackPolicy<TResult> : IAsyncPolicy<TResult>, IFallbackPolicy<TResult>
    {
    }
}
