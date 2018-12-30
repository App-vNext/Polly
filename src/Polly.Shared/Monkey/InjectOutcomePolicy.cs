using System;
using System.Threading;

namespace Polly.Monkey
{
    /// <summary>
    /// A policy that throws an exception in place of executing the passed delegate.
    /// <remarks>The policy can also be configured to return null in place of the exception, to explicitly fake that no exception is thrown.</remarks>
    /// </summary>
    public class InjectOutcomePolicy : MonkeyPolicy
    {
        private readonly Func<Context, Exception> _faultProvider;

        internal InjectOutcomePolicy(Func<Context, Exception> faultProvider, Func<Context, double> injectionRate, Func<Context, bool> enabled) 
            : base(injectionRate, enabled)
        {
            _faultProvider = faultProvider ?? throw new ArgumentNullException(nameof(faultProvider));
        }

        /// <inheritdoc/>
        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return MonkeyEngine.InjectExceptionImplementation(
                action,
                context,
                cancellationToken,
                _faultProvider,
                InjectionRate,
                Enabled);
        }
    }

    /// <summary>
    /// A policy that injects an outcome (throws an exception or returns a specific result), in place of executing the passed delegate.
    /// </summary>
    public class InjectOutcomePolicy<TResult> : MonkeyPolicy<TResult>
    {
        private readonly Func<Context, Exception> _faultProvider;
        private readonly Func<Context, TResult> _resultProvider;

        internal InjectOutcomePolicy(Func<Context, Exception> faultProvider, Func<Context, double> injectionRate, Func<Context, bool> enabled)
            : base(injectionRate, enabled)
        {
            _faultProvider = faultProvider ?? throw new ArgumentNullException(nameof(faultProvider));
        }

        internal InjectOutcomePolicy(Func<Context, TResult> resultProvider, Func<Context, double> injectionRate, Func<Context, bool> enabled)
            : base(injectionRate, enabled)
        {
            _resultProvider = resultProvider ?? throw new ArgumentNullException(nameof(resultProvider));
        }

        /// <inheritdoc/>
        protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            if (_faultProvider != null)
            {
                return MonkeyEngine.InjectExceptionImplementation(
                    action,
                    context,
                    cancellationToken,
                    _faultProvider,
                    InjectionRate,
                    Enabled);
            }
            else if (_resultProvider != null)
            {
                return MonkeyEngine.InjectResultImplementation(
                    action,
                    context,
                    cancellationToken,
                    _resultProvider,
                    InjectionRate,
                    Enabled);
            }
            else
            {
                throw new InvalidOperationException("Either a fault or fake result to inject must be defined.");
            }
        }
    }
}
