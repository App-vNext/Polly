namespace Polly.Timeout
{
    /// <summary>
    /// A timeout policy that can be applied to asynchronous delegate executions.
    /// </summary>
    public interface IAsyncTimeoutPolicy : IAsyncPolicy, ITimeoutPolicy
    {
    }

    /// <summary>
    /// A timeout policy that can be applied to asynchronous delegate executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface IAsyncTimeoutPolicy<TResult> : IAsyncPolicy<TResult>, ITimeoutPolicy<TResult>
    {
    }
}
