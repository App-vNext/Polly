using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly.Caching;
using Polly.Utilities;

namespace Polly.Specs.Helpers.Caching
{
    /// <summary>
    /// An intentionally naive stub cache implementation.  Its purpose is to be the simplest thing possible to support tests of the CachePolicy and CacheEngine, not a production-usable implementation.
    /// </summary>
    internal class StubCacheProvider : ISyncCacheProvider, IAsyncCacheProvider
    {
        class CacheItem
        {
            public CacheItem(object value, Ttl ttl)
            {
                Expiry = DateTimeOffset.MaxValue - SystemClock.DateTimeOffsetUtcNow() > ttl.Timespan ? SystemClock.DateTimeOffsetUtcNow().Add(ttl.Timespan) : DateTimeOffset.MaxValue;
                Value = value;
            }

            public readonly DateTimeOffset Expiry;
            public readonly object Value;
        }

        private readonly Dictionary<string, CacheItem> cachedValues = new Dictionary<string, CacheItem>();

        public object Get(string key)
        {
            if (cachedValues.ContainsKey(key))
            {
                if (SystemClock.DateTimeOffsetUtcNow() < cachedValues[key].Expiry)
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

        public void Put(string key, object value, Ttl ttl)
        {
            cachedValues[key] = new CacheItem(value, ttl);
        }

        #region Naive async-over-sync implementation

        // Intentionally naive async-over-sync implementation.  Its purpose is to be the simplest thing to support tests of the CachePolicyAsync and CacheEngineAsync, not to be a usable implementation of IAsyncCacheProvider.  
        public Task<object> GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return Task.FromResult(Get(key));
        }

        public Task PutAsync(string key, object value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            Put(key, value, ttl);
            return TaskHelper.EmptyTask;
        }

        #endregion

    }
}
