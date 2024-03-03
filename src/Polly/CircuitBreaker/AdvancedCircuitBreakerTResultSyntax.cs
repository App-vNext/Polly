namespace Polly;

/// <summary>
/// Fluent API for defining a Circuit Breaker <see cref="Policy{TResult}"/>.
/// </summary>
public static class AdvancedCircuitBreakerTResultSyntax
{
    /// <summary>
    /// <para> Builds a <see cref="Policy{TResult}"/> that will function like a Circuit Breaker.</para>
    /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception or result exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak" />; if no exception or handled result is encountered, the circuit will reset.
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
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak)
    {
        Action<DelegateResult<TResult>, TimeSpan> doNothingOnBreak = (_, _) => { };
        Action doNothingOnReset = () => { };

        return policyBuilder.AdvancedCircuitBreaker(
            failureThreshold, samplingDuration, minimumThroughput,
            durationOfBreak,
            doNothingOnBreak,
            doNothingOnReset);
    }

    /// <summary>
    /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception or result exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak" />; if no exception or handled result is encountered, the circuit will reset.
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
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
    /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
    /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
    /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
    /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset) =>
        policyBuilder.AdvancedCircuitBreaker(
            failureThreshold, samplingDuration, minimumThroughput,
            durationOfBreak,
            (outcome, timespan, _) => onBreak(outcome, timespan),
            _ => onReset());

    /// <summary>
    /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception or result exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak" />; if no exception or handled result is encountered, the circuit will reset.
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
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
    /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
    /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
    /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
    /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset)
    {
        Action doNothingOnHalfOpen = () => { };
        return policyBuilder.AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput,
            durationOfBreak,
            onBreak,
            onReset,
            doNothingOnHalfOpen);
    }

    /// <summary>
    /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration"/>, the proportion of actions resulting in a handled exception or result exceeds <paramref name="failureThreshold"/>, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak" />; if no exception or handled result is encountered, the circuit will reset.
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
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be greater than zero</exception>
    /// <exception cref="ArgumentOutOfRangeException">failureThreshold;Value must be less than or equal to one</exception>
    /// <exception cref="ArgumentOutOfRangeException">samplingDuration;Value must be equal to or greater than the minimum resolution of the CircuitBreaker timer</exception>
    /// <exception cref="ArgumentOutOfRangeException">minimumThroughput;Value must be greater than one</exception>
    /// <exception cref="ArgumentOutOfRangeException">durationOfBreak;Value must be greater than zero</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onHalfOpen"/> is <see langword="null"/>.</exception>
    public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset, Action onHalfOpen) =>
        policyBuilder.AdvancedCircuitBreaker(
            failureThreshold, samplingDuration, minimumThroughput,
            durationOfBreak,
            (outcome, timespan, _) => onBreak(outcome, timespan),
            _ => onReset(),
            onHalfOpen);

    /// <summary>
    /// <para> Builds a <see cref="Policy{TResult}" /> that will function like a Circuit Breaker.</para>
    /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration" />, the proportion of actions resulting in a handled exception or result exceeds <paramref name="failureThreshold" />, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak" />; if no exception or handled result is encountered, the circuit will reset.
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onHalfOpen"/> is <see langword="null"/>.</exception>
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold, TimeSpan samplingDuration, int minimumThroughput, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen) =>
        policyBuilder.AdvancedCircuitBreaker(
            failureThreshold, samplingDuration, minimumThroughput,
            durationOfBreak,
            (outcome, _, timespan, context) => onBreak(outcome, timespan, context),
            onReset,
            onHalfOpen);

    /// <summary>
    /// <para> Builds a <see cref="Policy{TResult}" /> that will function like a Circuit Breaker.</para>
    /// <para>The circuit will break if, within any timeslice of duration <paramref name="samplingDuration" />, the proportion of actions resulting in a handled exception or result exceeds <paramref name="failureThreshold" />, provided also that the number of actions through the circuit in the timeslice is at least <paramref name="minimumThroughput" />. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak" />. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException" /> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak" />; if no exception or handled result is encountered, the circuit will reset.
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onHalfOpen"/> is <see langword="null"/>.</exception>
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    public static CircuitBreakerPolicy<TResult> AdvancedCircuitBreaker<TResult>(
        this PolicyBuilder<TResult> policyBuilder,
        double failureThreshold,
        TimeSpan samplingDuration,
        int minimumThroughput,
        TimeSpan durationOfBreak,
        Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak,
        Action<Context> onReset,
        Action onHalfOpen)
    {
        var resolutionOfCircuit = TimeSpan.FromTicks(AdvancedCircuitController<EmptyStruct>.ResolutionOfCircuitTimer);

        if (failureThreshold <= 0)
            throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Value must be greater than zero.");
        if (failureThreshold > 1)
            throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Value must be less than or equal to one.");
        if (samplingDuration < resolutionOfCircuit)
            throw new ArgumentOutOfRangeException(nameof(samplingDuration), $"Value must be equal to or greater than {resolutionOfCircuit.TotalMilliseconds} milliseconds. This is the minimum resolution of the CircuitBreaker timer.");
        if (minimumThroughput <= 1)
            throw new ArgumentOutOfRangeException(nameof(minimumThroughput), "Value must be greater than one.");
        if (durationOfBreak < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(durationOfBreak), "Value must be greater than zero.");

        if (onBreak == null)
            throw new ArgumentNullException(nameof(onBreak));
        if (onReset == null)
            throw new ArgumentNullException(nameof(onReset));
        if (onHalfOpen == null)
            throw new ArgumentNullException(nameof(onHalfOpen));

        var breakerController = new AdvancedCircuitController<TResult>(
            failureThreshold,
            samplingDuration,
            minimumThroughput,
            durationOfBreak,
            onBreak,
            onReset,
            onHalfOpen);

        return new CircuitBreakerPolicy<TResult>(
            policyBuilder,
            breakerController);
    }
}
