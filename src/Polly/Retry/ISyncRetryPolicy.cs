namespace Polly.Retry
{
    /// <summary>
    /// A retry policy that can be applied to synchronous delegate executions.
    /// </summary>
    public interface ISyncRetryPolicy : ISyncPolicy, IRetryPolicy
    {
    }

    /// <summary>
    /// A retry policy that can be applied to synchronous delegate executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface ISyncRetryPolicy<TResult> : ISyncPolicy<TResult>, IRetryPolicy<TResult>
    {
    }
}
