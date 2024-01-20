namespace Polly.Utils;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable
#pragma warning disable CA5394 // Do not use insecure randomness
#pragma warning disable S2931 // Classes with "IDisposable" members should implement "IDisposable"

internal sealed class RandomUtil
{
    private readonly ThreadLocal<Random> _random;

    public static readonly RandomUtil Instance = new(null);

    public RandomUtil(int? seed) => _random = new ThreadLocal<Random>(() => seed == null ? new Random() : new Random(seed.Value));

    public double NextDouble() => _random.Value!.NextDouble();

    public int Next(int maxValue) => _random.Value!.Next(maxValue);

}
