namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;

internal static class AsyncAddBehaviourIfHandleSyntax
{
    internal static AsyncAddBehaviourIfHandlePolicy WithBehaviourAsync(
        this PolicyBuilder policyBuilder,
        Func<Exception, Task> behaviourIfHandle)
    {
        Guard.NotNull(behaviourIfHandle);

        return new AsyncAddBehaviourIfHandlePolicy(behaviourIfHandle, policyBuilder);
    }

    internal static AsyncAddBehaviourIfHandlePolicy<TResult> WithBehaviourAsync<TResult>(
        this PolicyBuilder<TResult> policyBuilder,
        Func<DelegateResult<TResult>, Task> behaviourIfHandle)
    {
        Guard.NotNull(behaviourIfHandle);

        return new AsyncAddBehaviourIfHandlePolicy<TResult>(behaviourIfHandle, policyBuilder);
    }
}
