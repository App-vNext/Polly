using System;
using System.Threading;

namespace Polly.Monkey
{
    internal static partial class MonkeyEngine
    {
        private static Random rand = new Random();

        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Action<Context, CancellationToken> fault,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (enabled(context) && GetRandomNumber() < injectionRate(context))
            {
                fault(context, cancellationToken);
            }

            return action(context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, Exception> fault,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (enabled(context) && GetRandomNumber() < injectionRate(context))
            {
                throw fault(context);
            }

            return action(context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, DelegateResult<TResult>> fault,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (enabled(context) && GetRandomNumber() < injectionRate(context))
            {
                DelegateResult<TResult> faultResponse = fault(context);
                if (faultResponse.Exception != null)
                {
                    throw faultResponse.Exception;
                }

                return faultResponse.Result;
            }

            return action(context, cancellationToken);
        }

        /// <summary>
        /// TODO: abstract this out @reminder: 1d
        /// Method to return a random number between 0 and 1
        /// </summary>
        /// <returns>a random <see cref="Double"/> between [0,1]</returns>
        private static Double GetRandomNumber()
        {
            return rand.NextDouble();
        }
    }
}
