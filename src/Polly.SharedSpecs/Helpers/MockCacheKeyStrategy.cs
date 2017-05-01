using System;
using Polly.Caching;

namespace Polly.Specs.Helpers
{
    internal class MockCacheKeyStrategy : ICacheKeyStrategy
    {
        private readonly Func<Context, string> strategy;

        public MockCacheKeyStrategy(Func<Context, string> strategy)
        {
            this.strategy = strategy;
        }

        public string GetCacheKey(Context context)
        {
            return strategy(context);
        }
    }
}
