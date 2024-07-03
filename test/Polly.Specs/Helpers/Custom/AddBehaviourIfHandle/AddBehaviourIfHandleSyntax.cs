namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;

internal static class AddBehaviourIfHandleSyntax
{
    internal static AddBehaviourIfHandlePolicy WithBehaviour(this PolicyBuilder policyBuilder, Action<Exception> behaviourIfHandle)
    {
        if (behaviourIfHandle == null)
        {
            throw new ArgumentNullException(nameof(behaviourIfHandle));
        }

        return new AddBehaviourIfHandlePolicy(behaviourIfHandle, policyBuilder);
    }

    internal static AddBehaviourIfHandlePolicy<TResult> WithBehaviour<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>> behaviourIfHandle)
    {
        if (behaviourIfHandle == null)
        {
            throw new ArgumentNullException(nameof(behaviourIfHandle));
        }

        return new AddBehaviourIfHandlePolicy<TResult>(behaviourIfHandle, policyBuilder);
    }
}
