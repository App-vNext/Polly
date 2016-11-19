using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly.Caching;
using Polly.Utilities;

namespace Polly.Specs.Helpers
{
    /// <summary>
    /// An intentionally naive stub cache implementation.  Its purpose is to be the simplest thing to support tests of the CachePolicy and CacheEngine, not to be a testable implementation of ICacheProvider.  
    /// </summary>
    internal class StubCacheProvider : ICacheProvider, ICacheProviderAsync
    {
        class CacheItem
        {
            public CacheItem(object value, TimeSpan ttl)
            {
                Expiry = DateTime.MaxValue - SystemClock.UtcNow() > ttl ? SystemClock.UtcNow().Add(ttl) : DateTime.MaxValue;
                Value = value;
            }

            public readonly DateTime Expiry;
            public readonly object Value;
        }

        private readonly Dictionary<string, CacheItem> cachedValues = new Dictionary<string, CacheItem>();

        public object Get(string key)
        {
            if (cachedValues.ContainsKey(key))
            {
                if (SystemClock.UtcNow() < cachedValues[key].Expiry)
                {
                    return cachedValues[key].Value;
                }
                else
                {
                    cachedValues.Remove(key);
                }
            }
            return null;
        }

        public void Put(string key, TimeSpan ttl, object value)
        {
            cachedValues[key] = new CacheItem(value, ttl);
        }

        #region Naive async-over-sync implementation

        // Intentionally naive async-over-sync implementation.  Its purpose is to be the simplest thing to support tests of the CachePolicyAsync and CacheEngineAsync, not to be a testable or usable implementation of ICacheProviderAsync.  

        public Task<object> GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return Task.FromResult(Get(key));
        }

        public Task PutAsync(string key, TimeSpan ttl, object value, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            Put(key, ttl, value);
            return TaskHelper.EmptyTask;
        }

        #endregion

    }
}
