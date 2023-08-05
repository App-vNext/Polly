namespace Polly.Simmy.Utils;

internal static class GuardExtensions
{
    public static void EnsureInjectionThreshold(this double injectionThreshold)
    {
        if (injectionThreshold < MonkeyStrategyConstants.MinInjectionThreshold)
        {
            throw new ArgumentOutOfRangeException(nameof(injectionThreshold), "Injection rate/threshold in Monkey strategies should always be a double between [0, 1]; never a negative number.");
        }

        if (injectionThreshold > MonkeyStrategyConstants.MaxInjectionThreshold)
        {
            throw new ArgumentOutOfRangeException(nameof(injectionThreshold), "Injection rate/threshold in Monkey strategies should always be a double between [0, 1]; never a number greater than 1.");
        }
    }
}
