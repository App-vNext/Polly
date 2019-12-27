namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to asynchronous delegate executions.
    /// </summary>
    public interface IAsyncRetryPolicy : IAsyncPolicy, IRetryPolicy
    {
    }

    /// <summary>
    /// A retry policy that can be applied to asynchronous delegate executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface IAsyncRetryPolicy<TResult> : IAsyncPolicy<TResult>, IRetryPolicy<TResult>
    {
    }
}
