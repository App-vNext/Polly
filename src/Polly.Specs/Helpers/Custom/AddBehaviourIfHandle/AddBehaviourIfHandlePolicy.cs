using System;
using System.Threading;

namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle
{
    internal class AddBehaviourIfHandlePolicy : Policy
    {
        private readonly Action<Exception> _behaviourIfHandle;

        internal AddBehaviourIfHandlePolicy(Action<Exception> behaviourIfHandle, PolicyBuilder policyBuilder)
            : base(policyBuilder)
        {
            _behaviourIfHandle = behaviourIfHandle ?? throw new ArgumentNullException(nameof(behaviourIfHandle));
        }

        protected override TResult SyncGenericImplementation<TExecutable, TResult>(in TExecutable action, Context context,
            CancellationToken cancellationToken)
        {
            return AddBehaviourIfHandleEngine.Implementation<TExecutable, TResult>(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates<TResult>.None,
                outcome => _behaviourIfHandle(outcome.Exception)
            );
        }
    }

    internal class AddBehaviourIfHandlePolicy<TResult> : Policy<TResult>
    {
        private readonly Action<DelegateResult<TResult>> _behaviourIfHandle;

        internal AddBehaviourIfHandlePolicy(
            Action<DelegateResult<TResult>> behaviourIfHandle,
            PolicyBuilder<TResult> policyBuilder)
            : base(policyBuilder)
        {
            _behaviourIfHandle = behaviourIfHandle ?? throw new ArgumentNullException(nameof(behaviourIfHandle));
        }

        protected override TResult SyncGenericImplementation<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            return AddBehaviourIfHandleEngine.Implementation(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates,
                _behaviourIfHandle
            );
        }
    }
}
