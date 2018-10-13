using System;
using System.Threading;
using Polly.Utilities;

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
            if (enabled(context) && RandomGenerator.GetRandomNumber() < injectionRate(context))
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
            if (enabled(context) && RandomGenerator.GetRandomNumber() < injectionRate(context))
            {
                throw fault(context);
            }

            return action(context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, TResult> fault,
            Func<Context, Double> injectionRate,
            Func<Context, bool> enabled)
        {
            if (enabled(context) && RandomGenerator.GetRandomNumber() < injectionRate(context))
            {
                return fault(context);
            }

            return action(context, cancellationToken);
        }
    }
}
