namespace Polly.Retry;

internal static class RetryHelper
{
    private const double JitterFactor = 0.5;

    private const double ExponentialFactor = 2.0;

    // Upper-bound to prevent overflow beyond TimeSpan.MaxValue. Potential truncation during conversion from double to long
    // (as described at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions)
    // is avoided by the arbitrary subtraction of 1,000.
    private static readonly double MaxTimeSpanTicks = (double)TimeSpan.MaxValue.Ticks - 1_000;

    public static bool IsValidDelay(TimeSpan delay) => delay >= TimeSpan.Zero;

    public static TimeSpan GetRetryDelay(
        DelayBackoffType type,
        bool jitter,
        int attempt,
        TimeSpan baseDelay,
        TimeSpan? maxDelay,
        ref double state,
        Func<double> randomizer)
    {
        try
        {
            var delay = GetRetryDelayCore(type, jitter, attempt, baseDelay, ref state, randomizer);

            // stryker disable once equality : no means to test this
            if (maxDelay is TimeSpan maxDelayValue && delay > maxDelayValue)
            {
                return maxDelay.Value;
            }

            return delay;
        }
        catch (OverflowException)
        {
            if (maxDelay is { } value)
            {
                return value;
            }

            return TimeSpan.MaxValue;
        }
    }

    private static TimeSpan GetRetryDelayCore(DelayBackoffType type, bool jitter, int attempt, TimeSpan baseDelay, ref double state, Func<double> randomizer)
    {
        if (baseDelay == TimeSpan.Zero)
        {
            return baseDelay;
        }

        if (jitter)
        {
            return type switch
            {
                DelayBackoffType.Constant => ApplyJitter(baseDelay, randomizer),
                DelayBackoffType.Linear => ApplyJitter(TimeSpan.FromMilliseconds((attempt + 1) * baseDelay.TotalMilliseconds), randomizer),
                DelayBackoffType.Exponential => DecorrelatedJitterBackoffV2(attempt, baseDelay, ref state, randomizer),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "The retry backoff type is not supported.")
            };
        }

        return type switch
        {
            DelayBackoffType.Constant => baseDelay,
#if !NETCOREAPP
            DelayBackoffType.Linear => TimeSpan.FromMilliseconds((attempt + 1) * baseDelay.TotalMilliseconds),
            DelayBackoffType.Exponential => TimeSpan.FromMilliseconds(Math.Pow(ExponentialFactor, attempt) * baseDelay.TotalMilliseconds),
#else
            DelayBackoffType.Linear => (attempt + 1) * baseDelay,
            DelayBackoffType.Exponential => Math.Pow(ExponentialFactor, attempt) * baseDelay,
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "The retry backoff type is not supported.")
        };
    }

    /// <summary>
    /// Generates sleep durations in an exponentially backing-off, jittered manner, making sure to mitigate any correlations.
    /// For example: 850ms, 1455ms, 3060ms.
    /// Per discussion in Polly issue https://github.com/App-vNext/Polly/issues/530, the jitter of this implementation exhibits fewer spikes and a smoother distribution than the AWS jitter formula.
    /// </summary>
    /// <param name="attempt">The current attempt.</param>
    /// <param name="baseDelay">The median delay to target before the first retry, call it <c>f (= f * 2^0).</c>
    /// Choose this value both to approximate the first delay, and to scale the remainder of the series.
    /// Subsequent retries will (over a large sample size) have a median approximating retries at time <c>f * 2^1, f * 2^2 ... f * 2^t</c> etc for try t.
    /// The actual amount of delay-before-retry for try t may be distributed between 0 and <c>f * (2^(t+1) - 2^(t-1)) for t >= 2;</c>
    /// or between 0 and <c>f * 2^(t+1)</c>, for t is 0 or 1.</param>
    /// <param name="prev">The previous state value used for calculations.</param>
    /// <param name="randomizer">The generator to use.</param>
    /// <remarks>
    /// This code was adopted from https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry/blob/master/src/Polly.Contrib.WaitAndRetry/Backoff.DecorrelatedJitterV2.cs.
    /// </remarks>
    private static TimeSpan DecorrelatedJitterBackoffV2(int attempt, TimeSpan baseDelay, ref double prev, Func<double> randomizer)
    {
        // The original author/credit for this jitter formula is @george-polevoy .
        // Jitter formula used with permission as described at https://github.com/App-vNext/Polly/issues/530#issuecomment-526555979
        // Minor adaptations (pFactor = 4.0 and rpScalingFactor = 1 / 1.4d) by @reisenberger, to scale the formula output for easier parameterization to users.

        // A factor used within the formula to help smooth the first calculated delay.
        const double PFactor = 4.0;

        // A factor used to scale the median values of the retry times generated by the formula to be _near_ whole seconds, to aid Polly user comprehension.
        // This factor allows the median values to fall approximately at 1, 2, 4 etc seconds, instead of 1.4, 2.8, 5.6, 11.2.
        const double RpScalingFactor = 1 / 1.4d;

        long targetTicksFirstDelay = baseDelay.Ticks;

        double t = attempt + randomizer();
        double next = Math.Pow(ExponentialFactor, t) * Math.Tanh(Math.Sqrt(PFactor * t));

        // At t >=1024, the above will tend to infinity which would otherwise cause the
        // ticks to go negative. See https://github.com/App-vNext/Polly/issues/2163.
        if (double.IsInfinity(next))
        {
            prev = next;
            return TimeSpan.FromTicks((long)MaxTimeSpanTicks);
        }

        double formulaIntrinsicValue = next - prev;
        prev = next;

        long ticks = (long)Math.Min(formulaIntrinsicValue * RpScalingFactor * targetTicksFirstDelay, MaxTimeSpanTicks);

#pragma warning disable S3236 // Remove this argument from the method call; it hides the caller information.
        Debug.Assert(ticks >= 0, "ticks cannot be negative");
#pragma warning restore S3236 // Remove this argument from the method call; it hides the caller information.

        return TimeSpan.FromTicks(ticks);
    }

#pragma warning disable IDE0047 // Remove unnecessary parentheses which offer less mental gymnastics
    private static TimeSpan ApplyJitter(TimeSpan delay, Func<double> randomizer)
    {
        var offset = (delay.TotalMilliseconds * JitterFactor) / 2;
        var randomDelay = (delay.TotalMilliseconds * JitterFactor * randomizer()) - offset;
        var newDelay = delay.TotalMilliseconds + randomDelay;

        return TimeSpan.FromMilliseconds(newDelay);
    }
#pragma warning restore IDE0047 // Remove unnecessary parentheses which offer less mental gymnastics
}
