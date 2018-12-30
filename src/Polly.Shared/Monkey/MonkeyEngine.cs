using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Monkey
{
    internal static class MonkeyEngine
    {
        private static bool ShouldInject(Context context, Func<Context, double> injectionRate, Func<Context, bool> enabled)
        {
            if (!enabled(context))
            {
                return false;
            }

            double injectionThreshold = injectionRate(context);
            if (injectionThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(injectionThreshold), "Injection rate/threshold in Monkey policies should always be a double between [0, 1]; never a negative number.");
            }
            if (injectionThreshold > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(injectionThreshold), "Injection rate/threshold in Monkey policies should always be a double between [0, 1]; never a number greater than 1.");
            }

            return ThreadSafeRandom_LockOncePerThread.NextDouble() < injectionThreshold;
        }

        internal static TResult InjectBehaviourImplementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Action<Context, CancellationToken> injectedBehaviour,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (ShouldInject(context, injectionRate, enabled))
            {
                injectedBehaviour(context, cancellationToken);
            }

            return action(context, cancellationToken);
        }

        internal static TResult InjectExceptionImplementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, Exception> injectedException,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (ShouldInject(context, injectionRate, enabled))
            {
                Exception exception = injectedException(context);
                if (exception != null)
                {
                    throw exception; 
                }
                
            }

            return action(context, cancellationToken);
        }

        internal static TResult InjectResultImplementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, TResult> injectedResult,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (ShouldInject(context, injectionRate, enabled))
            {
                return injectedResult(context);
            }

            return action(context, cancellationToken);
        }
    }
}
