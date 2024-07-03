namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;

internal static class AsyncAddBehaviourIfHandleSyntax
{
    internal static AsyncAddBehaviourIfHandlePolicy WithBehaviourAsync(
        this PolicyBuilder policyBuilder,
        Func<Exception, Task> behaviourIfHandle)
    {
        if (behaviourIfHandle == null)
        {
            throw new ArgumentNullException(nameof(behaviourIfHandle));
        }

        return new AsyncAddBehaviourIfHandlePolicy(behaviourIfHandle, policyBuilder);
    }

    internal static AsyncAddBehaviourIfHandlePolicy<TResult> WithBehaviourAsync<TResult>(
        this PolicyBuilder<TResult> policyBuilder,
        Func<DelegateResult<TResult>, Task> behaviourIfHandle)
    {
        if (behaviourIfHandle == null)
        {
            throw new ArgumentNullException(nameof(behaviourIfHandle));
        }

        return new AsyncAddBehaviourIfHandlePolicy<TResult>(behaviourIfHandle, policyBuilder);
    }
}
