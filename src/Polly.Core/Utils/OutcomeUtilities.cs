namespace Polly.Utils;

internal static class OutcomeUtilities
{
    /// <summary>
    /// Ensures that an <see cref="OperationCanceledException"/> escaping a strategy that substituted the
    /// execution <see cref="CancellationToken"/> (e.g. timeout or hedging) carries the caller's token when
    /// the cancellation was caused by that token.
    /// </summary>
    /// <typeparam name="T">The result type of the outcome.</typeparam>
    /// <param name="outcome">The outcome produced by the strategy.</param>
    /// <param name="callerToken">The cancellation token that was associated with the execution before the strategy substituted it.</param>
    /// <returns>
    /// An outcome whose <see cref="OperationCanceledException"/> carries <paramref name="callerToken"/> when the caller
    /// requested cancellation, preserving the original exception as its <see cref="Exception.InnerException"/>;
    /// otherwise the original <paramref name="outcome"/> unchanged.
    /// </returns>
    /// <remarks>
    /// The rewrite happens only when <paramref name="callerToken"/> actually requested cancellation. This preserves
    /// the contract that a real timeout, or an unrelated <see cref="OperationCanceledException"/> thrown while the
    /// caller's token was not cancelled, is left untouched.
    /// </remarks>
    public static Outcome<T> WithCallerCancellationToken<T>(this Outcome<T> outcome, CancellationToken callerToken)
    {
        if (callerToken.IsCancellationRequested && outcome.Exception is OperationCanceledException oce && oce.CancellationToken != callerToken)
        {
            return Outcome.FromException<T>(new OperationCanceledException(oce.Message, oce, callerToken).TrySetStackTrace());
        }

        return outcome;
    }
}
