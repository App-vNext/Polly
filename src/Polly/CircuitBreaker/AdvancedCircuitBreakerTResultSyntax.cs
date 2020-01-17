using System;
using Polly.CircuitBreaker;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Circuit Breaker <see cref="Policy{TResult}"/>. 
    /// </summary>
    public static class AdvancedCircuitBreakerTResultSyntax
    {
        private static void ValidateStaticDurationIsNonNegative(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("durationOfBreak", "Value must be greater than zero.");
            }
        }

        /// <summary>
        /// <para> Builds a <see cref="Policy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure).</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput, (_) => durationOfBreak);
        }

        /// <summary>
        /// <para> Builds a <see cref="Policy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for a dynamically calculated duration. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for a newly calculated duration; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure).</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, Func<int, TimeSpan> factoryForNextBreakDuration)
        {
            Action<DelegateResult<TResult>, TimeSpan> doNothingOnBreak = (_, __) => { };
            Action doNothingOnReset = () => { };

            return policyBuilder.AdvancedCircuitBreaker(
                failureThreshold, samplingDuration, minimumThroughput,
                factoryForNextBreakDuration,
                doNothingOnBreak,
                doNothingOnReset
                );
        }

        /// <summary>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput, (_) => durationOfBreak, onBreak, onReset);
        }

        /// <summary>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for a dynamically calculated duration. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for a newly calculated duration; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset)
            => policyBuilder.AdvancedCircuitBreaker(
                failureThreshold, samplingDuration, minimumThroughput,
                factoryForNextBreakDuration,
                (outcome, timespan, context) => onBreak(outcome, timespan),
                context => onReset()
                );

        /// <summary>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput, (_) => durationOfBreak, onBreak, onReset);
        }

        /// <summary>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for a dynamically calculated duration. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for a newly calculated duration; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset)
        {
            Action doNothingOnHalfOpen = () => { };
            return policyBuilder.AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput,
                factoryForNextBreakDuration,
                onBreak,
                onReset,
                doNothingOnHalfOpen
                );
        }

        /// <summary>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset, Action onHalfOpen)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput, (_) => durationOfBreak, onBreak, onReset, onHalfOpen);
        }

        /// <summary>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for a dynamically calculated duration. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for a newly calculated duration; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset, Action onHalfOpen)
            => policyBuilder.AdvancedCircuitBreaker(
                failureThreshold, samplingDuration, minimumThroughput,
                factoryForNextBreakDuration,
                (outcome, timespan, context) => onBreak(outcome, timespan),
                context => onReset(),
                onHalfOpen
                );

        /// <summary>
        /// <para> Builds a <see cref="Policy{TResult}" /> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open" /> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed" /> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen" /> state, ready to try action executions again.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput, (_) => durationOfBreak, onBreak, onReset, onHalfOpen);
        }

        /// <summary>
        /// <para> Builds a <see cref="Policy{TResult}" /> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for a dynamically calculated duration. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for a newly calculated duration; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open" /> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed" /> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen" /> state, ready to try action executions again.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
            => policyBuilder.AdvancedCircuitBreaker(
                failureThreshold, samplingDuration, minimumThroughput,
                factoryForNextBreakDuration,
                (outcome, state, timespan, context) => onBreak(outcome, timespan, context),
                onReset,
                onHalfOpen
            );

        /// <summary>
        /// <para> Builds a <see cref="Policy{TResult}" /> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open" /> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed" /> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen" /> state, ready to try action executions again.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput, (_) => durationOfBreak, onBreak, onReset, onHalfOpen);
        }

        /// <summary>
        /// <para> Builds a <see cref="Policy{TResult}" /> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a failure exceeds <paramref name="failureThreshold"/>,
        /// provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />.
        /// </para>
        /// <para>The circuit will stay broken for a dynamically calculated duration. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for a newly calculated duration; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open" /> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed" /> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen" /> state, ready to try action executions again.</param>
        /// <returns>The policy instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            var resolutionOfCircuit = TimeSpan.FromTicks(AdvancedCircuitController<EmptyStruct>.ResolutionOfCircuitTimer);

            if (failureThreshold <= 0) { throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Value must be greater than zero."); }
            if (failureThreshold > 1) { throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Value must be less than or equal to one."); }
            if (samplingDuration < resolutionOfCircuit) { throw new ArgumentOutOfRangeException(nameof(samplingDuration), $"Value must be equal to or greater than {resolutionOfCircuit.TotalMilliseconds} milliseconds. This is the minimum resolution of the CircuitBreaker timer."); }
            if (minimumThroughput <= 1) { throw new ArgumentOutOfRangeException(nameof(minimumThroughput), "Value must be greater than one."); }

            if (onBreak == null) { throw new ArgumentNullException(nameof(onBreak)); }
            if (onReset == null) { throw new ArgumentNullException(nameof(onReset)); }
            if (onHalfOpen == null) { throw new ArgumentNullException(nameof(onHalfOpen)); }

            var breakerController = new AdvancedCircuitController<TResult>(
                failureThreshold,
                samplingDuration,
                minimumThroughput,
                factoryForNextBreakDuration,
                onBreak,
                onReset,
                onHalfOpen);
            return new CircuitBreakerPolicy<TResult>(
                policyBuilder,
                breakerController
                );
        }
    }
}
