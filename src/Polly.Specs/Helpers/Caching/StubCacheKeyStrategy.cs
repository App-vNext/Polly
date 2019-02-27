using System;
using Polly.Caching;

namespace Polly.Specs.Helpers.Caching
{
    /// <summary>
    /// A configurable stub ICacheKeyStrategy, to support tests..
    /// </summary>
    internal class StubCacheKeyStrategy : ICacheKeyStrategy
    {
        private readonly Func<Context, string> strategy;

        public StubCacheKeyStrategy(Func<Context, string> strategy)
        {
            this.strategy = strategy;
        }

        public string GetCacheKey(Context context)
        {
            return strategy(context);
        }
    }
}
