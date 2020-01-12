﻿using System;
using System.Diagnostics;
using System.Threading;

namespace Polly
{
    public abstract partial class Policy<TResult> : ISyncPolicyInternal<TResult>
    {
        [DebuggerStepThrough]
        TResult ISyncPolicyInternal<TResult>.Execute<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

            try
            {
                return SyncGenericImplementation(action, context, cancellationToken);
            }
            finally
            {
                RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
            }
        }

        [DebuggerStepThrough]
        PolicyResult<TResult> ISyncPolicyInternal<TResult>.ExecuteAndCapture<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                TResult result = ((ISyncPolicyInternal<TResult>)this).Execute(action, context, cancellationToken);

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
