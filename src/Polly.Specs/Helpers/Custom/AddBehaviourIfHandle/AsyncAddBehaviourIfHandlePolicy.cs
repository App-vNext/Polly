using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers.Custom.AddBehaviourIfHandle
{
    internal class AsyncAddBehaviourIfHandlePolicy : AsyncPolicy
    {
        private readonly Func<Exception, Task> _behaviourIfHandle;

        internal AsyncAddBehaviourIfHandlePolicy(
            Func<Exception, Task> behaviourIfHandle, 
            PolicyBuilder policyBuilder)
            : base(policyBuilder)
        {
            _behaviourIfHandle = behaviourIfHandle ?? throw new ArgumentNullException(nameof(behaviourIfHandle));
        }

        protected override Task<TResult> AsyncGenericImplementation<TExecutableAsync, TResult>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncAddBehaviourIfHandleEngine.ImplementationAsync(
                action,
                context,
                cancellationToken,
                continueOnCapturedContext,
                ExceptionPredicates,
                ResultPredicates<TResult>.None,
                outcome => _behaviourIfHandle(outcome.Exception)
            );
        }
    }

    internal class AsyncAddBehaviourIfHandlePolicy<TResult> : AsyncPolicy<TResult>
    {
        private readonly Func<DelegateResult<TResult>, Task> _behaviourIfHandle;

        internal AsyncAddBehaviourIfHandlePolicy(
            Func<DelegateResult<TResult>, Task> behaviourIfHandle,
            PolicyBuilder<TResult> policyBuilder)
            : base(policyBuilder)
        {
            _behaviourIfHandle = behaviourIfHandle ?? throw new ArgumentNullException(nameof(behaviourIfHandle));

        }

        protected override Task<TResult> AsyncGenericImplementation<TExecutableAsync>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncAddBehaviourIfHandleEngine.ImplementationAsync(
                action,
                context,
                cancellationToken,
                continueOnCapturedContext,
                ExceptionPredicates,
                ResultPredicates,
                _behaviourIfHandle);
        }
    }
}
