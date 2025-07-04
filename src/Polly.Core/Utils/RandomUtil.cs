namespace Polly.Utils;

#pragma warning disable CA5394 // Do not use insecure randomness

internal static class RandomUtil
{
#if NET
    public static double NextDouble() => Random.Shared.NextDouble();
    public static int Next(int maxValue) => Random.Shared.Next(maxValue);
#else
    private static readonly ThreadLocal<Random> Instance = new(() => new Random());

    public static double NextDouble() => Instance.Value.NextDouble();
    public static int Next(int maxValue) => Instance.Value.Next(maxValue);
#endif
}
