using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    public abstract partial class AsyncPolicyV8 : IAsyncPolicyInternal
    {
        async Task IAsyncPolicyInternal.ExecuteAsync(IAsyncExecutable action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                await AsyncNonGenericImplementationV8(action, context, cancellationToken, continueOnCapturedContext)
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
                return await AsyncGenericImplementationV8<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        async Task<PolicyResult> IAsyncPolicyInternal.ExecuteAndCaptureAsync(IAsyncExecutable action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
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
