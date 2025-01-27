namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;

internal static class AddBehaviourIfHandleSyntax
{
    internal static AddBehaviourIfHandlePolicy WithBehaviour(this PolicyBuilder policyBuilder, Action<Exception> behaviourIfHandle)
    {
        behaviourIfHandle.ShouldNotBeNull();
        return new AddBehaviourIfHandlePolicy(behaviourIfHandle, policyBuilder);
    }

    internal static AddBehaviourIfHandlePolicy<TResult> WithBehaviour<TResult>(this PolicyBuilder<TResult> policyBuilder, Action<DelegateResult<TResult>> behaviourIfHandle)
    {
        behaviourIfHandle.ShouldNotBeNull();
        return new AddBehaviourIfHandlePolicy<TResult>(behaviourIfHandle, policyBuilder);
    }
}
