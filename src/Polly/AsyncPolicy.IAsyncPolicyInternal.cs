using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    public abstract partial class AsyncPolicy : IAsyncPolicyInternal
    {
        async Task IAsyncPolicyInternal.ExecuteAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                await AsyncNonGenericImplementation(action, context, cancellationToken, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        async Task<TResult> IAsyncPolicyInternal.ExecuteAsync<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                return await AsyncGenericImplementation<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        async Task<PolicyResult> IAsyncPolicyInternal.ExecuteAndCaptureAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                await ((IAsyncPolicyInternal)this).ExecuteAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
                return PolicyResult.Successful(context);
            }
            catch (Exception exception)
            {
                return PolicyResult.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }

        async Task<PolicyResult<TResult>> IAsyncPolicyInternal.ExecuteAndCaptureAsync<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                return PolicyResult<TResult>.Successful(
                    await ((IAsyncPolicyInternal)this).ExecuteAsync<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext), 
                    context);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }
    }
}
