using System;
using System.Linq;
using Polly.CircuitBreaker;
using Polly.Utilities;

namespace Polly
{
    /// <summary>
    /// Fluent API for defining a Circuit Breaker <see cref="Policy"/>. 
    /// </summary>
    public static class AdvancedCircuitBreakerSyntax
    {
        /// <summary>
        /// <para> Builds a <see cref="Policy"/> that will function like a Circuit Breaker.</para>
        /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure).</param>
        /// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        /// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        /// <returns>The policy instance.</returns>
        /// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        public static CircuitBreakerPolicy AdvancedCircuitBreaker(this PolicyBuilder policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak)
        {
            Action<Exception, TimeSpan> doNothingOnBreak = (_, __) => { };
            Action doNothingOnReset = () => { };

            return policyBuilder.AdvancedCircuitBreaker(
                failureThreshold, samplingDuration, minimumThroughput, 
                durationOfBreak, 
                doNothingOnBreak,
                doNothingOnReset
                );
        }

        /// <summary>
        /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no exception is thrown, the circuit will reset.
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
        public static CircuitBreakerPolicy AdvancedCircuitBreaker(this PolicyBuilder policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<Exception, TimeSpan> onBreak, Action onReset)
        {
            return policyBuilder.AdvancedCircuitBreaker(
                failureThreshold, samplingDuration, minimumThroughput, 
                durationOfBreak, 
                (exception, timespan, context) => onBreak(exception, timespan), 
                context => onReset()
                );
        }

        /// <summary>
        /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no exception is thrown, the circuit will reset.
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
        public static CircuitBreakerPolicy AdvancedCircuitBreaker(this PolicyBuilder policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset)
        {
            Action doNothingOnHalfOpen = () => { };
            return policyBuilder.AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput, 
                durationOfBreak, 
                onBreak, 
                onReset,
                doNothingOnHalfOpen
                );
        }

        /// <summary>
        /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no exception is thrown, the circuit will reset.
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
        public static CircuitBreakerPolicy AdvancedCircuitBreaker(this PolicyBuilder policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<Exception, TimeSpan> onBreak, Action onReset, Action onHalfOpen)
        {
            return policyBuilder.AdvancedCircuitBreaker(
                failureThreshold, samplingDuration, minimumThroughput, 
                durationOfBreak,
                (exception, timespan, context) => onBreak(exception, timespan),
                context => onReset(),
                onHalfOpen
                );
        }

        /// <summary>
        /// <para> Builds a <see cref="Policy" /> that will function like a Circuit Breaker.</para>
        /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
        /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception
        /// that broke the circuit.
        /// </para>
        /// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        /// again for another <paramref name="durationOfBreak" />; if no exception is thrown, the circuit will reset.
        /// </para>
        /// </summary>
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
        public static CircuitBreakerPolicy AdvancedCircuitBreaker(this PolicyBuilder policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        {
            var resolutionOfCircuit = TimeSpan.FromTicks(AdvancedCircuitController<EmptyStruct>.ResolutionOfCircuitTimer);

            if (failureThreshold <= 0) throw new ArgumentOutOfRangeException("failureThreshold", "Value must be greater than zero.");
            if (failureThreshold > 1) throw new ArgumentOutOfRangeException("failureThreshold", "Value must be less than or equal to one.");
            if (samplingDuration < resolutionOfCircuit) throw new ArgumentOutOfRangeException("samplingDuration", String.Format("Value must be equal to or greater than {0} milliseconds. This is the minimum resolution of the CircuitBreaker timer.", resolutionOfCircuit.TotalMilliseconds));
            if (minimumThroughput <= 1) throw new ArgumentOutOfRangeException("minimumThroughput", "Value must be greater than one.");
            if (durationOfBreak < TimeSpan.Zero) throw new ArgumentOutOfRangeException("durationOfBreak", "Value must be greater than zero.");

            if (onBreak == null) throw new ArgumentNullException("onBreak");
            if (onReset == null) throw new ArgumentNullException("onReset");
            if (onHalfOpen == null) throw new ArgumentNullException("onHalfOpen");

            var breakerController = new AdvancedCircuitController<EmptyStruct>(
                failureThreshold, 
                samplingDuration, 
                minimumThroughput, 
                durationOfBreak, 
                (outcome, timespan, context) => onBreak(outcome.Exception, timespan, context), 
                onReset, 
                onHalfOpen);
            return new CircuitBreakerPolicy(
                (action, context, cancellationToken) => CircuitBreakerEngine.Implementation<EmptyStruct>(
                    (ctx, ct) => { action(ctx, ct); return EmptyStruct.Instance; },
                    context,
                    cancellationToken,
                    policyBuilder.ExceptionPredicates, 
                    PredicateHelper<EmptyStruct>.EmptyResultPredicates, 
                    breakerController), 
                policyBuilder.ExceptionPredicates, 
                breakerController
                );
        }

        // For v560OnBreakDelegate : delete the overload above, and replace with the two below

        ///// <summary>
        ///// <para> Builds a <see cref="Policy" /> that will function like a Circuit Breaker.</para>
        ///// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
        ///// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        ///// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception
        ///// that broke the circuit.
        ///// </para>
        ///// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        ///// again for another <paramref name="durationOfBreak" />; if no exception is thrown, the circuit will reset.
        ///// </para>
        ///// </summary>
        ///// <param name="policyBuilder">The policy builder.</param>
        ///// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        ///// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        ///// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        ///// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        ///// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open" /> state.</param>
        ///// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed" /> state.</param>
        ///// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen" /> state, ready to try action executions again.</param>
        ///// <returns>The policy instance.</returns>
        ///// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        ///// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        ///// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        ///// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        ///// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        ///// <exception cref="ArgumentNullException">onBreak</exception>
        ///// <exception cref="ArgumentNullException">onReset</exception>
        ///// <exception cref="ArgumentNullException">onHalfOpen</exception>
        ///// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        //public static CircuitBreakerPolicy AdvancedCircuitBreaker(this PolicyBuilder policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<Exception, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        //{
        //    return policyBuilder.AdvancedCircuitBreaker(
        //        failureThreshold, samplingDuration, minimumThroughput,
        //        durationOfBreak,
        //        (exception, state, timespan, context) => onBreak(exception, timespan, context),
        //        onReset,
        //        onHalfOpen
        //    );
        //}

        ///// <summary>
        ///// <para> Builds a <see cref="Policy" /> that will function like a Circuit Breaker.</para>
        ///// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
        ///// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
        ///// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception
        ///// that broke the circuit.
        ///// </para>
        ///// <para>If the first action after the break duration period results in a handled exception, the circuit will break
        ///// again for another <paramref name="durationOfBreak" />; if no exception is thrown, the circuit will reset.
        ///// </para>
        ///// </summary>
        ///// <param name="policyBuilder">The policy builder.</param>
        ///// <param name="failureThreshold">The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.</param>
        ///// <param name="samplingDuration">The duration of the timeslice over which failure ratios are assessed.</param>
        ///// <param name="minimumThroughput">The minimum throughput: this many actions or more must pass through the circuit in the timeslice, for statistics to be considered significant and the circuit-breaker to come into action.</param>
        ///// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
        ///// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open" /> state.</param>
        ///// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed" /> state.</param>
        ///// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen" /> state, ready to try action executions again.</param>
        ///// <returns>The policy instance.</returns>
        ///// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
        ///// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
        ///// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
        ///// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
        ///// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
        ///// <exception cref="ArgumentNullException">onBreak</exception>
        ///// <exception cref="ArgumentNullException">onReset</exception>
        ///// <exception cref="ArgumentNullException">onHalfOpen</exception>
        ///// <remarks>(see "Release It!" by Michael T. Nygard fi)</remarks>
        //public static CircuitBreakerPolicy AdvancedCircuitBreaker(this PolicyBuilder policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<Exception, CircuitState, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
        //{
        //    var resolutionOfCircuit = TimeSpan.FromTicks(AdvancedCircuitController<EmptyStruct>.ResolutionOfCircuitTimer);

        //    if (failureThreshold <= 0) throw new ArgumentOutOfRangeException("failureThreshold", "Value must be greater than zero.");
        //    if (failureThreshold > 1) throw new ArgumentOutOfRangeException("failureThreshold", "Value must be less than or equal to one.");
        //    if (samplingDuration < resolutionOfCircuit) throw new ArgumentOutOfRangeException("samplingDuration", String.Format("Value must be equal to or greater than {0} milliseconds. This is the minimum resolution of the CircuitBreaker timer.", resolutionOfCircuit.TotalMilliseconds));
        //    if (minimumThroughput <= 1) throw new ArgumentOutOfRangeException("minimumThroughput", "Value must be greater than one.");
        //    if (durationOfBreak < TimeSpan.Zero) throw new ArgumentOutOfRangeException("durationOfBreak", "Value must be greater than zero.");

        //    if (onBreak == null) throw new ArgumentNullException("onBreak");
        //    if (onReset == null) throw new ArgumentNullException("onReset");
        //    if (onHalfOpen == null) throw new ArgumentNullException("onHalfOpen");

        //    var breakerController = new AdvancedCircuitController<EmptyStruct>(
        //        failureThreshold, 
        //        samplingDuration, 
        //        minimumThroughput, 
        //        durationOfBreak, 
        //        (outcome, state, timespan, context) => onBreak(outcome.Exception, state, timespan, context), 
        //        onReset, 
        //        onHalfOpen);
        //    return new CircuitBreakerPolicy(
        //        (action, context, cancellationToken) => CircuitBreakerEngine.Implementation<EmptyStruct>(
        //            (ctx, ct) => { action(ctx, ct); return EmptyStruct.Instance; },
        //            context,
        //            cancellationToken,
        //            policyBuilder.ExceptionPredicates, 
        //            PredicateHelper<EmptyStruct>.EmptyResultPredicates, 
        //            breakerController), 
        //        policyBuilder.ExceptionPredicates, 
        //        breakerController
        //        );
        //}

    }
}
