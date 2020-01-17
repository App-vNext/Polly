﻿using System;
using Polly.CircuitBreaker;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Circuit Breaker <see cref="AsyncPolicy{TResult}"/>. 
    /// </summary>
    public static class AsyncCircuitBreakerTResultSyntax
    {
        private static void ValidateStaticDurationIsNonNegative(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("durationOfBreak", "Value must be greater than zero.");
            }
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, TimeSpan durationOfBreak)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.CircuitBreakerAsync(failureThreshold, (_) => durationOfBreak);
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
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
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, Func<int, TimeSpan> factoryForNextBreakDuration)
        {
            Action<DelegateResult<TResult>, TimeSpan> doNothingOnBreak = (_, __) => { };
            Action doNothingOnReset = () => { };

            return policyBuilder.CircuitBreakerAsync(
               failureThreshold,
               factoryForNextBreakDuration,
               doNothingOnBreak,
               doNothingOnReset
               );
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.CircuitBreakerAsync(failureThreshold, (_) => durationOfBreak, onBreak, onReset);
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
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
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset)
            => policyBuilder.CircuitBreakerAsync(
                failureThreshold,
                factoryForNextBreakDuration,
                (outcome, timespan, context) => onBreak(outcome, timespan),
                context => onReset()
                );

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.CircuitBreakerAsync(failureThreshold, (_) => durationOfBreak, onBreak, onReset);
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
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
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset)
        {
            Action doNothingOnHalfOpen = () => { };
            return policyBuilder.CircuitBreakerAsync(
                failureThreshold,
                factoryForNextBreakDuration,
                onBreak,
                onReset,
                doNothingOnHalfOpen
                );
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset, Action onHalfOpen)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.CircuitBreakerAsync(failureThreshold, (_) => durationOfBreak, onBreak, onReset, onHalfOpen);
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
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
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset, Action onHalfOpen)
            => policyBuilder.CircuitBreakerAsync(
                failureThreshold,
                factoryForNextBreakDuration,
                (outcome, timespan, context) => onBreak(outcome, timespan),
                context => onReset(),
                onHalfOpen
                );

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.CircuitBreakerAsync(failureThreshold, (_) => durationOfBreak, onBreak, onReset, onHalfOpen);
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
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
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
            => policyBuilder.CircuitBreakerAsync(
                failureThreshold,
                factoryForNextBreakDuration,
                (outcome, state, timespan, context) => onBreak(outcome, timespan, context),
                onReset,
                onHalfOpen
            );

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
        /// </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result 
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a failure, the circuit will break
        /// again for another <paramref name="durationOfBreak"/>; if no failure is encountered, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            ValidateStaticDurationIsNonNegative(durationOfBreak);
            return policyBuilder.CircuitBreakerAsync(failureThreshold, (_) => durationOfBreak, onBreak, onReset, onHalfOpen);
        }

        /// <summary>
        /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
        /// <para>
        /// The circuit will break if the number of consecutive actions resulting in a failure exceeds <paramref name="failureThreshold"/>.
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
        /// <param name="failureThreshold">The number of failures that are allowed before opening the circuit.</param>
        /// <param name="factoryForNextBreakDuration">A function to calculate the duration the circuit will stay open before resetting, based on the number of consecutive CircuitFailures. Re-evaluated each time the circuit Opens.</param>
        /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
        /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
        /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero.</exception>
        /// <exception cref="InvalidOperationException">factoryForNextBreakDuration;Generated value must be always be non-negative. Exception would be thrown when invoking policy, if duration were negative.</exception>
        /// <exception cref="ArgumentNullException">onBreak</exception>
        /// <exception cref="ArgumentNullException">onReset</exception>
        /// <exception cref="ArgumentNullException">onHalfOpen</exception>
        public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int failureThreshold, Func<int, TimeSpan> factoryForNextBreakDuration, Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            if (failureThreshold <= 0) { throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Value must be greater than zero."); }


            if (onBreak == null) { throw new ArgumentNullException(nameof(onBreak)); }
            if (onReset == null) { throw new ArgumentNullException(nameof(onReset)); }
            if (onHalfOpen == null) { throw new ArgumentNullException(nameof(onHalfOpen)); }

            var breakerController = new ConsecutiveCountCircuitController<TResult>(
                failureThreshold,
                factoryForNextBreakDuration,
                onBreak,
                onReset,
                onHalfOpen);
            return new AsyncCircuitBreakerPolicy<TResult>(
                policyBuilder,
                breakerController
            );
        }
    }
}

