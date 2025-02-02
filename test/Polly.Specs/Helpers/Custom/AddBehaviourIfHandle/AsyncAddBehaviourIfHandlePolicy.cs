namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle;

internal class AsyncAddBehaviourIfHandlePolicy : AsyncPolicy
{
    private readonly Func<Exception, Task> _behaviourIfHandle;

    internal AsyncAddBehaviourIfHandlePolicy(
        Func<Exception, Task> behaviourIfHandle,
        PolicyBuilder policyBuilder)
        : base(policyBuilder) =>
        _behaviourIfHandle = behaviourIfHandle ?? throw new ArgumentNullException(nameof(behaviourIfHandle));

    protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncAddBehaviourIfHandleEngine.ImplementationAsync(
            ExceptionPredicates,
            ResultPredicates<TResult>.None,
            outcome => _behaviourIfHandle(outcome.Exception),
            action,
            context,
            cancellationToken,
            continueOnCapturedContext);
}

internal class AsyncAddBehaviourIfHandlePolicy<TResult> : AsyncPolicy<TResult>
{
    private readonly Func<DelegateResult<TResult>, Task> _behaviourIfHandle;

    internal AsyncAddBehaviourIfHandlePolicy(
        Func<DelegateResult<TResult>, Task> behaviourIfHandle,
        PolicyBuilder<TResult> policyBuilder)
        : base(policyBuilder) =>
        _behaviourIfHandle = behaviourIfHandle ?? throw new ArgumentNullException(nameof(behaviourIfHandle));

    protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncAddBehaviourIfHandleEngine.ImplementationAsync(
            ExceptionPredicates,
            ResultPredicates,
            _behaviourIfHandle,
            action,
            context,
            cancellationToken,
            continueOnCapturedContext);
}
