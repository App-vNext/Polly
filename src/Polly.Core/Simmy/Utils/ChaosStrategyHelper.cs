namespace Polly.Simmy.Utils;

internal static class ChaosStrategyHelper
{
    public static async ValueTask<bool> ShouldInjectAsync(
        ResilienceContext context,
        Func<InjectionRateGeneratorArguments, ValueTask<double>> injectionRateGenerator,
        Func<EnabledGeneratorArguments, ValueTask<bool>> enabledGenerator,
        Func<double> randomizer)
    {
        Guard.NotNull(context);

        // to prevent executing config delegates if token was signaled before to start.
        context.CancellationToken.ThrowIfCancellationRequested();

        if (!await enabledGenerator(new(context)).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            return false;
        }

        // to prevent executing InjectionRate config delegate if token was signaled on Enabled configuration delegate.
        context.CancellationToken.ThrowIfCancellationRequested();

        double injectionThreshold = await injectionRateGenerator(new(context)).ConfigureAwait(context.ContinueOnCapturedContext);

        // to prevent executing further config delegates if token was signaled on InjectionRate configuration delegate.
        context.CancellationToken.ThrowIfCancellationRequested();

        injectionThreshold = CoerceInjectionThreshold(injectionThreshold);
        return randomizer() < injectionThreshold;
    }

    private static double CoerceInjectionThreshold(double injectionThreshold)
    {
        // stryker disable once equality : no means to test this
        if (injectionThreshold < ChaosStrategyConstants.MinInjectionThreshold)
        {
            return ChaosStrategyConstants.MinInjectionThreshold;
        }

        // stryker disable once equality : no means to test this
        if (injectionThreshold > ChaosStrategyConstants.MaxInjectionThreshold)
        {
            return ChaosStrategyConstants.MaxInjectionThreshold;
        }

        return injectionThreshold;
    }
}
