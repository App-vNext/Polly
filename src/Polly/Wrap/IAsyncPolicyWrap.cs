namespace Polly.Wrap
{
    /// <summary>
    /// A wrapper for composing policies that can be applied to asynchronous executions.
    /// </summary>
    public interface IAsyncPolicyWrap : IAsyncPolicy, IPolicyWrap
    {
    }

    /// <summary>
    /// A wrapper for composing policies that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface IAsyncPolicyWrap<TResult> : IAsyncPolicy<TResult>, IPolicyWrap<TResult>
    {
    }
}
