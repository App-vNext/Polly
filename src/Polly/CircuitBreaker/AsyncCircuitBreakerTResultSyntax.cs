namespace Polly;

/// <summary>
/// Fluent API for defining a Circuit Breaker <see cref="AsyncPolicy{TResult}"/>.
/// </summary>
public static class AsyncCircuitBreakerTResultSyntax
{
    /// <summary>
    /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
    /// <para>The circuit will break if <paramref name="handledEventsAllowedBeforeBreaking"/>
    /// exceptions or results that are handled by this policy are encountered consecutively. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak"/>; if no exception or handled result is encountered, the circuit will reset.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="handledEventsAllowedBeforeBreaking">The number of exceptions or handled results that are allowed before opening the circuit.</param>
    /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
    /// <returns>The policy instance.</returns>
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">handledEventsAllowedBeforeBreaking;Value must be greater than zero.</exception>
    public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        Action<DelegateResult<TResult>, TimeSpan> doNothingOnBreak = (_, _) => { };
        Action doNothingOnReset = () => { };

        return policyBuilder.CircuitBreakerAsync(
           handledEventsAllowedBeforeBreaking,
           durationOfBreak,
           doNothingOnBreak,
           doNothingOnReset);
    }

    /// <summary>
    /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
    /// <para>The circuit will break if <paramref name="handledEventsAllowedBeforeBreaking"/>
    /// exceptions or results that are handled by this policy are encountered consecutively. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak"/>; if no exception or handled result is encountered, the circuit will reset.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="handledEventsAllowedBeforeBreaking">The number of exceptions or handled results that are allowed before opening the circuit.</param>
    /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
    /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
    /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
    /// <returns>The policy instance.</returns>
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">handledEventsAllowedBeforeBreaking;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset) =>
        policyBuilder.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking,
            durationOfBreak,
            (outcome, timespan, _) => onBreak(outcome, timespan),
            _ => onReset());

    /// <summary>
    /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
    /// <para>The circuit will break if <paramref name="handledEventsAllowedBeforeBreaking"/>
    /// exceptions or results that are handled by this policy are encountered consecutively. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak"/>; if no exception or handled result is encountered, the circuit will reset.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="handledEventsAllowedBeforeBreaking">The number of exceptions or handled results that are allowed before opening the circuit.</param>
    /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
    /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
    /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
    /// <returns>The policy instance.</returns>
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">handledEventsAllowedBeforeBreaking;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset)
    {
        Action doNothingOnHalfOpen = () => { };

        return policyBuilder.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking,
            durationOfBreak,
            onBreak,
            onReset,
            doNothingOnHalfOpen);
    }

    /// <summary>
    /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
    /// <para>The circuit will break if <paramref name="handledEventsAllowedBeforeBreaking"/>
    /// exceptions or results that are handled by this policy are encountered consecutively. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak"/>; if no exception or handled result is encountered, the circuit will reset.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="handledEventsAllowedBeforeBreaking">The number of exceptions or handled results that are allowed before opening the circuit.</param>
    /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
    /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
    /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
    /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
    /// <returns>The policy instance.</returns>
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">handledEventsAllowedBeforeBreaking;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan> onBreak, Action onReset, Action onHalfOpen) =>
        policyBuilder.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking,
            durationOfBreak,
            (outcome, timespan, _) => onBreak(outcome, timespan),
            _ => onReset(),
            onHalfOpen);

    /// <summary>
    /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
    /// <para>The circuit will break if <paramref name="handledEventsAllowedBeforeBreaking"/>
    /// exceptions or results that are handled by this policy are encountered consecutively. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak"/>; if no exception or handled result is encountered, the circuit will reset.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="handledEventsAllowedBeforeBreaking">The number of exceptions or handled results that are allowed before opening the circuit.</param>
    /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
    /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
    /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
    /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
    /// <returns>The policy instance.</returns>
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">handledEventsAllowedBeforeBreaking;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onHalfOpen"/> is <see langword="null"/>.</exception>
    public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen) =>
        policyBuilder.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking,
            durationOfBreak,
            (outcome, _, timespan, context) => onBreak(outcome, timespan, context),
            onReset,
            onHalfOpen);

    /// <summary>
    /// <para> Builds a <see cref="AsyncPolicy{TResult}"/> that will function like a Circuit Breaker.</para>
    /// <para>The circuit will break if <paramref name="handledEventsAllowedBeforeBreaking"/>
    /// exceptions or results that are handled by this policy are encountered consecutively. </para>
    /// <para>The circuit will stay broken for the <paramref name="durationOfBreak"/>. Any attempt to execute this policy
    /// while the circuit is broken, will immediately throw a <see cref="BrokenCircuitException"/> containing the exception or result
    /// that broke the circuit.
    /// </para>
    /// <para>If the first action after the break duration period results in a handled exception or result, the circuit will break
    /// again for another <paramref name="durationOfBreak"/>; if no exception or handled result is encountered, the circuit will reset.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    /// <param name="policyBuilder">The policy builder.</param>
    /// <param name="handledEventsAllowedBeforeBreaking">The number of exceptions or handled results that are allowed before opening the circuit.</param>
    /// <param name="durationOfBreak">The duration the circuit will stay open before resetting.</param>
    /// <param name="onBreak">The action to call when the circuit transitions to an <see cref="CircuitState.Open"/> state.</param>
    /// <param name="onReset">The action to call when the circuit resets to a <see cref="CircuitState.Closed"/> state.</param>
    /// <param name="onHalfOpen">The action to call when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again. </param>
    /// <returns>The policy instance.</returns>
    /// <remarks>(see "Release It!" by Michael T. Nygard fi).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">handledEventsAllowedBeforeBreaking;Value must be greater than zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onBreak"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onReset"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onHalfOpen"/> is <see langword="null"/>.</exception>
    public static AsyncCircuitBreakerPolicy<TResult> CircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context> onBreak, Action<Context> onReset, Action onHalfOpen)
    {
        if (handledEventsAllowedBeforeBreaking <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(handledEventsAllowedBeforeBreaking), "Value must be greater than zero.");
        }

        if (durationOfBreak < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(durationOfBreak), "Value must be greater than zero.");
        }

        if (onBreak == null)
        {
            throw new ArgumentNullException(nameof(onBreak));
        }

        if (onReset == null)
        {
            throw new ArgumentNullException(nameof(onReset));
        }

        if (onHalfOpen == null)
        {
            throw new ArgumentNullException(nameof(onHalfOpen));
        }

        var breakerController = new ConsecutiveCountCircuitController<TResult>(
            handledEventsAllowedBeforeBreaking,
            durationOfBreak,
            onBreak,
            onReset,
            onHalfOpen);

        return new AsyncCircuitBreakerPolicy<TResult>(
            policyBuilder,
            breakerController);
    }
}

