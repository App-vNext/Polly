using Polly.Simmy.Utils;

namespace Polly.Simmy;

#pragma warning disable CA1031 // Do not catch general exception types

/// <summary>
/// Contains common functionality for chaos strategies which intentionally disrupt executions - which monkey around with calls.
/// </summary>
public abstract class MonkeyStrategy : ResilienceStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MonkeyStrategy"/> class.
    /// </summary>
    /// <param name="injectionRate">Delegate that determines the injection rate.</param>
    /// <param name="enabled">Delegate that determines whether or not the chaos is enabled.</param>
    protected MonkeyStrategy(
        Func<ResilienceContext, ValueTask<double>> injectionRate, Func<ResilienceContext, ValueTask<bool>> enabled)
    {
        Guard.NotNull(enabled);
        Guard.NotNull(injectionRate);

        InjectionRate = injectionRate;
        Enabled = enabled;
    }

    internal Func<ResilienceContext, ValueTask<double>> InjectionRate { get; }

    internal Func<ResilienceContext, ValueTask<bool>> Enabled { get; }

    internal async ValueTask<bool> ShouldInject(ResilienceContext context, RandomUtil randomUtil)
    {
        Guard.NotNull(randomUtil);

        // to prevent executing config delegates if token was signaled before to start.
        context.CancellationToken.ThrowIfCancellationRequested();

        if (!await Enabled(context).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            return false;
        }

        // to prevent executing InjectionRate config delegate if token was signaled on Enabled configuration delegate.
        context.CancellationToken.ThrowIfCancellationRequested();

        double injectionThreshold = await InjectionRate(context).ConfigureAwait(context.ContinueOnCapturedContext);

        // to prevent executing further config delegates if token was signaled on InjectionRate configuration delegate.
        context.CancellationToken.ThrowIfCancellationRequested();

        injectionThreshold.EnsureInjectionThreshold();
        return randomUtil.NextDouble() < injectionThreshold;
    }
}
