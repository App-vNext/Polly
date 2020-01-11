using System;
using Polly.CircuitBreaker;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Circuit Breaker <see cref="AsyncPolicy"/>. 
    /// </summary>
    public static class AsyncCircuitBreakerSyntax
    {
        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy"/> that will function like a Circuit Breaker.</para>
        /// <para>The circuit will break if <paramref name="exceptionsAllowedBeforeBreaking"/>
        /// exceptions that are handled by this policy are raised consecutively. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="exceptionsAllowedBeforeBreaking">The number of exceptions that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">exceptionsAllowedBeforeBreaking;Value must be greater than zero.</exception>
        public static IAsyncCircuitBreakerPolicy CircuitBreakerAsync(this PolicyBuilder policyBuilder, int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            return policyBuilder.CircuitBreakerAsync(
               exceptionsAllowedBeforeBreaking,
               durationOfBreak,
               null,
               (Action) null
               );
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy"/> that will function like a Circuit Breaker.</para>
        /// <para>The circuit will break if <paramref name="exceptionsAllowedBeforeBreaking"/>
        /// exceptions that are handled by this policy are raised consecutively. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="exceptionsAllowedBeforeBreaking">The number of exceptions that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">exceptionsAllowedBeforeBreaking;Value must be greater than zero.</exception>
        public static IAsyncCircuitBreakerPolicy CircuitBreakerAsync(this PolicyBuilder policyBuilder, int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<Exception, TimeSpan> onBreak, Action onReset)
            => policyBuilder.CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking,
                durationOfBreak,
                onBreak: onBreak == null ? (Action<Exception, TimeSpan, Context>)null : (exception, timespan, context) => onBreak(exception, timespan),
                onReset: onReset == null ? (Action<Context>)null : context => onReset()
                );

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy"/> that will function like a Circuit Breaker.</para>
        /// <para>The circuit will break if <paramref name="exceptionsAllowedBeforeBreaking"/>
        /// exceptions that are handled by this policy are raised consecutively. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="exceptionsAllowedBeforeBreaking">The number of exceptions that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">exceptionsAllowedBeforeBreaking;Value must be greater than zero.</exception>
        public static IAsyncCircuitBreakerPolicy CircuitBreakerAsync(this PolicyBuilder policyBuilder, int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset)
        {
            return policyBuilder.CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking, 
                durationOfBreak, 
                onBreak, 
                onReset,
                onHalfOpen: null
                );
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy"/> that will function like a Circuit Breaker.</para>
        /// <para>The circuit will break if <paramref name="exceptionsAllowedBeforeBreaking"/>
        /// exceptions that are handled by this policy are raised consecutively. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="exceptionsAllowedBeforeBreaking">The number of exceptions that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">exceptionsAllowedBeforeBreaking;Value must be greater than zero.</exception>
        public static IAsyncCircuitBreakerPolicy CircuitBreakerAsync(this PolicyBuilder policyBuilder, int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<Exception, TimeSpan> onBreak, Action onReset, Action onHalfOpen)
            => policyBuilder.CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking,
                durationOfBreak,
                onBreak: onBreak == null ? (Action<Exception, TimeSpan, Context>)null : (exception, timespan, context) => onBreak(exception, timespan),
                onReset: onReset == null ? (Action<Context>)null : context => onReset(),
                onHalfOpen
                );

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy"/> that will function like a Circuit Breaker.</para>
        /// <para>The circuit will break if <paramref name="exceptionsAllowedBeforeBreaking"/>
        /// exceptions that are handled by this policy are raised consecutively. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="exceptionsAllowedBeforeBreaking">The number of exceptions that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">exceptionsAllowedBeforeBreaking;Value must be greater than zero.</exception>
        public static IAsyncCircuitBreakerPolicy CircuitBreakerAsync(this PolicyBuilder policyBuilder, int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
            => policyBuilder.CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking,
                durationOfBreak,
                onBreak: onBreak == null ? (Action<Exception, CircuitState, TimeSpan, Context>)null : (exception, state, timespan, context) => onBreak(exception, timespan, context),
                onReset,
                onHalfOpen
            );

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy"/> that will function like a Circuit Breaker.</para>
        /// <para>The circuit will break if <paramref name="exceptionsAllowedBeforeBreaking"/>
        /// exceptions that are handled by this policy are raised consecutively. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="exceptionsAllowedBeforeBreaking">The number of exceptions that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">exceptionsAllowedBeforeBreaking;Value must be greater than zero.</exception>
        public static IAsyncCircuitBreakerPolicy CircuitBreakerAsync(this PolicyBuilder policyBuilder, int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<Exception, CircuitState, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            if (exceptionsAllowedBeforeBreaking <= 0) throw new ArgumentOutOfRangeException(nameof(exceptionsAllowedBeforeBreaking), "Value must be greater than zero.");
            if (durationOfBreak < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(durationOfBreak), "Value must be greater than zero.");

            var breakerController = new ConsecutiveCountCircuitController<object>(
                exceptionsAllowedBeforeBreaking,
                durationOfBreak,
                onBreak: onBreak == null ? (Action<DelegateResult<object>, CircuitState, TimeSpan, Context>)null : (outcome, state, timespan, context) => onBreak(outcome.Exception, state, timespan, context),
                onReset,
                onHalfOpen);
            return new AsyncCircuitBreakerPolicy(
                policyBuilder,
                breakerController
            );
        }
    }
}

