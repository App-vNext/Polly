using System;
using System.Threading;

namespace Polly.Monkey
{
    /// <summary>
    /// A policy that injects any custom behaviour before the execution of delegates.
    /// </summary>
    public class InjectBehaviourPolicy : MonkeyPolicy
    {
        private readonly Action<Context> _behaviour;

        internal InjectBehaviourPolicy(Action<Context> behaviour, Func<Context, double> injectionRate, Func<Context, bool> enabled) 
            : base(injectionRate, enabled)
        {
            _behaviour = behaviour ?? throw new ArgumentNullException(nameof(behaviour));
        }

        /// <inheritdoc/>
        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return MonkeyEngine.InjectBehaviourImplementation(
                action,
                context,
                cancellationToken,
                (ctx, ct) => _behaviour(ctx),
                InjectionRate,
                Enabled);
        }
    }
    /// <summary>
    /// A policy that injects any custom behaviour before the execution of delegates returning <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    public class InjectBehaviourPolicy<TResult> : MonkeyPolicy<TResult>
    {
        private readonly Action<Context> _behaviour;
        internal InjectBehaviourPolicy(Action<Context> behaviour, Func<Context, double> injectionRate, Func<Context, bool> enabled)
            : base(injectionRate, enabled)
        {
            _behaviour = behaviour ?? throw new ArgumentNullException(nameof(behaviour));
        }

        /// <inheritdoc/>
        protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return MonkeyEngine.InjectBehaviourImplementation(
                action,
                context,
                cancellationToken,
                (ctx, ct) => _behaviour(ctx),
                InjectionRate,
                Enabled);
        }
    }
}
