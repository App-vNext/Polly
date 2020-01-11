using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    public abstract partial class AsyncPolicyV8<TResult> : IAsyncPolicyInternal<TResult>
    {
        [DebuggerStepThrough]
        async Task<TResult> IAsyncPolicyInternal<TResult>.ExecuteAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                return await AsyncGenericImplementationV8(action, context, cancellationToken, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        [DebuggerStepThrough]
        async Task<PolicyResult<TResult>> IAsyncPolicyInternal<TResult>.ExecuteAndCaptureAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                TResult result = await ((IAsyncPolicyInternal<TResult>)this).ExecuteAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);

                if (ResultPredicates.AnyMatch(result))
                {
                    return PolicyResult<TResult>.Failure(result, context);
                }

                return PolicyResult<TResult>.Successful(result, context);
            }
            catch (Exception exception)
            {
                return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
            }
        }
    }
}
