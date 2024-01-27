namespace Polly.Specs.Helpers.Caching;

/// <summary>
/// A configurable stub ICacheKeyStrategy, to support tests..
/// </summary>
internal class StubCacheKeyStrategy : ICacheKeyStrategy
{
    private readonly Func<Context, string> _strategy;

    public StubCacheKeyStrategy(Func<Context, string> strategy) =>
        _strategy = strategy;

    public string GetCacheKey(Context context) =>
        _strategy(context);
}
