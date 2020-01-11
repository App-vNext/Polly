namespace Polly
{
    /// <summary>
    /// Transient exception handling policies that can be applied to asynchronous delegates
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public abstract class AsyncPolicy<TResult>
    {
        // Retained only because referenced in intellisense.  Will be deleted when AsyncPolicyV8 renamed to AsyncPolicy.
    }
}
