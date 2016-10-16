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
        private readonly Dictionary<string, object> cachedValues = new Dictionary<string, object>();

        public object Get(string key)
        {
            if (cachedValues.ContainsKey(key))
            {
                return cachedValues[key];
            }
            return null;
        }

        public void Put(string key, object value)
        {
            cachedValues[key] = value;
        }

        #region Naive async-over-sync implementation

        // Intentionally naive async-over-sync implementation.  Its purpose is to be the simplest thing to support tests of the CachePolicyAsync and CacheEngineAsync, not to be a testable or usable implementation of ICacheProviderAsync.  

        public Task<object> GetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return Task.FromResult(Get(key));
        }

        public Task PutAsync(string key, object value, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            Put(key, value);
            return TaskHelper.EmptyTask;
        }

        #endregion

    }
}
