using System;
using System.Diagnostics;
using System.Threading;

namespace Polly
{
    public abstract partial class Policy : ISyncPolicyInternal
    {
        [DebuggerStepThrough]
        void ISyncPolicyInternal.Execute<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                SyncNonGenericImplementation(action, context, cancellationToken);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        [DebuggerStepThrough]
        TResult ISyncPolicyInternal.Execute<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                return SyncGenericImplementation<TExecutable, TResult>(action, context, cancellationToken);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        [DebuggerStepThrough]
        PolicyResult ISyncPolicyInternal.ExecuteAndCapture<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                ((ISyncPolicyInternal) this).Execute(action, context, cancellationToken);
                return PolicyResult.Successful(context);
            }
            catch (Exception exception)
            {
                return PolicyResult.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        [DebuggerStepThrough]
        PolicyResult<TResult> ISyncPolicyInternal.ExecuteAndCapture<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                return PolicyResult<TResult>.Successful(((ISyncPolicyInternal) this).Execute<TExecutable, TResult>(action, context, cancellationToken), 
                    context);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }
    }
}
