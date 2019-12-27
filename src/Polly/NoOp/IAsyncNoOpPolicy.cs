namespace Polly.NoOp
{
    /// <summary>
    /// A no-op policy that can be applied to asynchronous executions.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    public interface IAsyncNoOpPolicy : IAsyncPolicy, INoOpPolicy
    {
    }

    /// <summary>
    /// A no-op policy that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface IAsyncNoOpPolicy<TResult> : IAsyncPolicy<TResult>, INoOpPolicy<TResult>
    {
    }
}
