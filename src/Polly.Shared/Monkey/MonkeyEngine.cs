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
            Action<Context> fault,
            Func<Context, Decimal> injectionRate,
            Func<Context, bool> enabled)
        {
            if (enabled(context) && GetRandomNumber() < injectionRate(context))
            {
                fault(context);
            }

            return action(context, cancellationToken);
        }

        internal static TResult Implementation<TResult>(
            Func<Context, CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            Func<Context, Exception> fault,
            Func<Context, Decimal> injectionRate,
            Func<Context, bool> enabled)
        {
            if (enabled(context) && GetRandomNumber() < injectionRate(context))
            {
                throw fault(context);
            }

            return action(context, cancellationToken);
        }

        /// <summary>
        /// Method to return a random number between 0 and 1
        /// </summary>
        /// <returns>a random <see cref="Decimal"/> between [0,1]</returns>
        private static Decimal GetRandomNumber()
        {
            return (Decimal)rand.NextDouble();
        }
    }
}
